using System.Collections.Generic;
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
        Task<IList<AssetOwnership>> UpdateAssetOwnership(List<Transaction> newTransactions);
    }

    public class AssetTracker : IAssetTracker
    {
        private readonly IAssetRepository _assetRepository;
        private readonly IAssetExchangeService _assetService;
        private readonly IMapper _mapper;

        public AssetTracker(IAssetRepository assetRepository, IServiceFactory serviceFactory, IMapper mapper)
        {
            _assetRepository = assetRepository;
            _assetService = serviceFactory.CreateAssetExchangeService();
            _mapper = mapper;
        }

        public async Task<IList<AssetOwnership>> UpdateAssetOwnership(List<Transaction> newTransactions)
        {
            var assetOwnerships = new List<AssetOwnership>();

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
                    assetOwnerships.Add(ownership);
                }
                else if (newTransaction.TransactionType == TransactionType.AssetTrade)
                {
                    var assetTrade = (AssetTradeTransaction) newTransaction;
                    var asset = await GetOrAddAsset(assetTrade.AssetNxtId);
                    var ownership = CreateAssetOwnership(assetTrade.QuantityQnt, newTransaction, asset);
                    assetOwnerships.Add(ownership);
                }
                else if (newTransaction.TransactionType == TransactionType.AssetTransfer)
                {
                    var attachment = (ColoredCoinsAssetTransferAttachment) newTransaction.Attachment;
                    var asset = await GetOrAddAsset(attachment.AssetId);
                    var ownership = CreateAssetOwnership(attachment.QuantityQnt, newTransaction, asset);
                    assetOwnerships.Add(ownership);
                }
                else if (newTransaction.TransactionType == TransactionType.AssetDelete)
                {
                    var attachment = (ColoredCoinsDeleteAttachment) newTransaction.Attachment;
                    var asset = await GetOrAddAsset(attachment.AssetId);
                    var ownership = CreateAssetOwnership(attachment.QuantityQnt, newTransaction, asset);
                    assetOwnerships.Add(ownership);
                }
            }
            return assetOwnerships;
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
