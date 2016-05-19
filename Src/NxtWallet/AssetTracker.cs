﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AutoMapper;
using NxtLib;
using NxtLib.AssetExchange;
using NxtWallet.Model;
using NxtWallet.ViewModel.Model;
using Asset = NxtWallet.ViewModel.Model.Asset;
using Transaction = NxtWallet.ViewModel.Model.Transaction;
using System.Linq;

namespace NxtWallet
{
    public interface IAssetTracker
    {
        Task UpdateAssetOwnership(List<Transaction> newTransactions);
        Task<AssetOwnership> GetOwnership(ulong nxtAssetId, int height);
        Task<long> GetAssetQuantity(ulong nxtAssetId, int height);
        Task SaveOwnerships();
        Task<IEnumerable<Asset>> GetOwnedAssetsSince(int blockHeight);
    }

    public class AssetTracker : IAssetTracker
    {
        private readonly IAssetRepository _assetRepository;
        private readonly IAssetExchangeService _assetService;
        private readonly IMapper _mapper;
        private readonly IBalanceCalculator _balanceCalculator;
        private readonly List<AssetOwnership> _newOwnerships = new List<AssetOwnership>();

        public IReadOnlyCollection<AssetOwnership> NewOwnerships { get; }

        public AssetTracker(IAssetRepository assetRepository, IServiceFactory serviceFactory, IMapper mapper,
            IBalanceCalculator balanceCalculator)
        {
            NewOwnerships = new ReadOnlyCollection<AssetOwnership>(_newOwnerships);

            _assetRepository = assetRepository;
            _assetService = serviceFactory.CreateAssetExchangeService();
            _mapper = mapper;
            _balanceCalculator = balanceCalculator;
        }

        public async Task UpdateAssetOwnership(List<Transaction> newTransactions)
        {
            _newOwnerships.Clear();

            foreach (var newTransaction in newTransactions)
            {
                if (newTransaction.TransactionType == TransactionType.AssetIssuance)
                {
                    var attachment = (ColoredCoinsAssetIssuanceAttachment) newTransaction.Attachment;
                    var savedAsset = await _assetRepository.SaveAssetAsync(new Asset
                    {
                        Decimals = attachment.Decimals,
                        Name = attachment.Name,
                        NxtId = (long)newTransaction.NxtId
                    });
                    var ownership = CreateAssetOwnership(attachment.QuantityQnt, newTransaction, savedAsset);
                    _newOwnerships.Add(ownership);
                }
                else if (newTransaction.TransactionType == TransactionType.AssetTrade)
                {
                    var assetTrade = (AssetTradeTransaction) newTransaction;
                    var asset = await GetOrAddAsset(assetTrade.AssetNxtId);
                    var deltaQuantity = assetTrade.QuantityQnt*(newTransaction.NqtAmount > 0 ? -1 : 1);
                    var ownership = CreateAssetOwnership(deltaQuantity, newTransaction, asset);
                    _newOwnerships.Add(ownership);
                }
                else if (newTransaction.TransactionType == TransactionType.AssetTransfer)
                {
                    var attachment = (ColoredCoinsAssetTransferAttachment) newTransaction.Attachment;
                    var asset = await GetOrAddAsset(attachment.AssetId);
                    var deltaQuantity = attachment.QuantityQnt * (newTransaction.UserIsTransactionRecipient ? 1 : -1);
                    var ownership = CreateAssetOwnership(deltaQuantity, newTransaction, asset);
                    _newOwnerships.Add(ownership);
                }
                else if (newTransaction.TransactionType == TransactionType.AssetDelete)
                {
                    var attachment = (ColoredCoinsDeleteAttachment) newTransaction.Attachment;
                    var asset = await GetOrAddAsset(attachment.AssetId);
                    var ownership = CreateAssetOwnership(attachment.QuantityQnt * -1, newTransaction, asset);
                    _newOwnerships.Add(ownership);
                }
            }

            if (_newOwnerships.Any())
            {
                var updatedOwnerships = await CalculateOwnership(_newOwnerships);
                await _assetRepository.UpdatesAssetOwnershipBalancesAsync(updatedOwnerships);
            }
        }

        public async Task<AssetOwnership> GetOwnership(ulong nxtAssetId, int height)
        {
            var dbOwnership = await _assetRepository.GetAssetOwnershipAtHeightAsync((long) nxtAssetId, height);
            if (_newOwnerships.Any())
            {
                var asset = await _assetRepository.GetAssetAsync(nxtAssetId);
                var newOwnership = _newOwnerships.OrderByDescending(o => o.Height)
                    .FirstOrDefault(o => o.Height < height && o.AssetId == asset.Id);
                if (newOwnership != null && (dbOwnership == null || newOwnership.Height > dbOwnership.Height))
                {
                    return newOwnership;
                }
            }
            return dbOwnership;
        }

        public async Task<long> GetAssetQuantity(ulong nxtAssetId, int height)
        {
            var nxtAsset = await _assetService.GetAsset(nxtAssetId);
            if (nxtAsset.InitialQuantityQnt != nxtAsset.QuantityQnt)
            {
                var assetDeletesReply = await _assetService.GetAssetDeletes(AssetIdOrAccountId.ByAssetId(nxtAssetId));
                var deletedQnt = assetDeletesReply.Deletes.Where(d => d.Height < height).Sum(d => d.QuantityQnt);
                return nxtAsset.InitialQuantityQnt - deletedQnt;
            }
            return nxtAsset.QuantityQnt;
        }

        public async Task SaveOwnerships()
        {
            _newOwnerships.ForEach(o => o.TransactionId = o.Transaction.Id);
            await _assetRepository.SaveAssetOwnershipsAsync(_newOwnerships);
        }

        public async Task<IEnumerable<Asset>> GetOwnedAssetsSince(int height)
        {
            var dbOwnershipsOwnedAtHeight = (await _assetRepository.GetOwnershipsOwnedAtHeight(height)).ToList();
            var dbOwnershipsSinceHeight = (await _assetRepository.GetOwnershipsOwnedSinceHeightAsync(height)).ToList();

            var newOwnershipsOwnedAtHeight = GetNewownershipsOwnedAtHeight(height);
            var newOwhershipsSinceHeight = GetOwnershipsOwnedSinceHeight(height);

            // Not 100% accurate, we might get more than we asked for
            var assetIds = dbOwnershipsOwnedAtHeight.Select(o => o.AssetId)
                .Union(dbOwnershipsSinceHeight.Select(o => o.AssetId))
                .Union(newOwnershipsOwnedAtHeight.Select(o => o.AssetId))
                .Union(newOwhershipsSinceHeight.Select(o => o.AssetId))
                .Distinct();

            var assets = await _assetRepository.GetAssetsAsync(assetIds);
            return assets;
        }

        private List<AssetOwnership> GetOwnershipsOwnedSinceHeight(int height)
        {
            var grouped = (from o in NewOwnerships
                where o.Height >= height
                group o by o.AssetId
                into grouping
                select new
                {
                    AssetId = grouping.Key,
                    Ownership = grouping.FirstOrDefault(g => g.BalanceQnt > 0)
                }).ToList();

            return grouped.Select(g => _mapper.Map<AssetOwnership>(g.Ownership)).ToList();
        }

        private List<AssetOwnership> GetNewownershipsOwnedAtHeight(int height)
        {
            var grouped = (from o in NewOwnerships
                where o.Height < height
                group o by o.AssetId
                into grouping
                select new
                {
                    AssetId = grouping.Key,
                    Ownership = grouping.OrderByDescending(g => g.Height).FirstOrDefault()
                }).ToList();

            return grouped
                .Where(g => g.Ownership.BalanceQnt > 0)
                .Select(g => _mapper.Map<AssetOwnership>(g.Ownership))
                .ToList();
        }

        private async Task<IEnumerable<AssetOwnership>> CalculateOwnership(IEnumerable<AssetOwnership> assetOwnerships)
        {
            var updatedOwnerships = new List<AssetOwnership>();

            var ownershipsByAsset = assetOwnerships
                .GroupBy(o => o.AssetId, ownership => ownership)
                .ToDictionary(o => o.Key, grouping => grouping.ToList());

            foreach (var assetId in ownershipsByAsset.Keys)
            {
                var ownershipList = ownershipsByAsset[assetId];
                var existingOwnerships = await _assetRepository.GetAssetOwnershipsAsync(assetId, 0);
                var allOrderedOwnerships = existingOwnerships.Union(ownershipList).Distinct().OrderBy(o => o.Height).ToList();
                updatedOwnerships.AddRange(_balanceCalculator.Calculate(ownershipList, new List<AssetOwnership>(), allOrderedOwnerships));
            }
            return updatedOwnerships;
        }

        private async Task<Asset> GetOrAddAsset(ulong assetId)
        {
            var asset = await _assetRepository.GetAssetAsync(assetId);
            if (asset == null)
            {
                asset = _mapper.Map<Asset>(await _assetService.GetAsset(assetId));
                asset = await _assetRepository.SaveAssetAsync(asset);
            }
            return asset;
        }

        private static AssetOwnership CreateAssetOwnership(long quantityQnt, Transaction transaction, Asset asset)
        {
            var assetOwnership = new AssetOwnership
            {
                AssetDecimals = asset.Decimals,
                AssetId = asset.Id,
                QuantityQnt = quantityQnt,
                Height = transaction.Height,
                Transaction = transaction
            };
            return assetOwnership;
        }
    }
}