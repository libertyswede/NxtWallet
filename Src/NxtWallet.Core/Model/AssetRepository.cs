using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Data.Entity;
using NxtWallet.Core.ViewModel.Model;
using NxtWallet.Core.Migrations.Model;

namespace NxtWallet.Core.Model
{
    public interface IAssetRepository
    {
        Task<Asset> GetAssetAsync(ulong nxtId);
        Task<IEnumerable<Asset>> GetAssetsAsync(IEnumerable<int> assetIds);
        Task<Asset> SaveAssetAsync(Asset asset);
        Task<IEnumerable<AssetOwnership>> GetAssetOwnershipsAsync(int assetId, int minHeight);
        Task<AssetOwnership> GetAssetOwnershipAtHeightAsync(long nxtAssetId, int height);
        Task SaveAssetOwnershipAsync(AssetOwnership assetOwnership);
        Task SaveAssetOwnershipsAsync(List<AssetOwnership> newOwnerships);
        Task UpdatesAssetOwnershipBalancesAsync(IEnumerable<AssetOwnership> updatedOwnerships);
        Task<IEnumerable<AssetOwnership>> GetOwnershipsOwnedAtHeight(int height);
        Task<IEnumerable<AssetOwnership>> GetOwnershipsOwnedSinceHeightAsync(int height);
    }

    public class AssetRepository : IAssetRepository
    {
        private readonly IMapper _mapper;

        public AssetRepository(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<Asset> GetAssetAsync(ulong nxtId)
        {
            using (var context = new WalletContext())
            {
                var assetDto = await context.Assets.SingleOrDefaultAsync(a => a.NxtId == (long)nxtId);
                return _mapper.Map<Asset>(assetDto);
            }
        }

        public async Task<IEnumerable<Asset>> GetAssetsAsync(IEnumerable<int> assetIds)
        {
            using (var context = new WalletContext())
            {
                var assetDtos = await context.Assets.Where(a => assetIds.Contains(a.Id)).ToListAsync();
                return _mapper.Map<IEnumerable<Asset>>(assetDtos);
            }
        }

        public async Task<Asset> SaveAssetAsync(Asset asset)
        {
            var assetDto = _mapper.Map<AssetDto>(asset);
            using (var context = new WalletContext())
            {
                var existingAsset = await context.Assets.SingleOrDefaultAsync(a => a.NxtId == assetDto.NxtId);
                if (existingAsset != null)
                    return _mapper.Map<Asset>(existingAsset);

                context.Assets.Add(assetDto);
                await context.SaveChangesAsync();
                return _mapper.Map<Asset>(assetDto);
            }
        }

        public async Task<IEnumerable<AssetOwnership>> GetAssetOwnershipsAsync(int assetId, int minHeight)
        {
            using (var context = new WalletContext())
            {
                var ownerships = await context.AssetOwnerships
                    .Where(o => o.AssetId == assetId && o.Height >= minHeight)
                    .ToListAsync();
                return _mapper.Map<IEnumerable<AssetOwnership>>(ownerships);
            }
        }

        public async Task<IEnumerable<AssetOwnership>> GetOwnershipsOwnedAtHeight(int height)
        {
            using (var context = new WalletContext())
            {
                var grouped = await (from o in context.AssetOwnerships
                    where o.Height < height
                    group o by o.AssetId
                    into grouping
                    select new
                    {
                        AssetId = grouping.Key,
                        Ownership = grouping.OrderByDescending(g => g.Height).FirstOrDefault()
                    }).ToListAsync();

                return grouped
                    .Where(g => g.Ownership.BalanceQnt > 0)
                    .Select(g => _mapper.Map<AssetOwnership>(g.Ownership));
            }
        }

        public async Task<IEnumerable<AssetOwnership>> GetOwnershipsOwnedSinceHeightAsync(int height)
        {
            using (var context = new WalletContext())
            {
                var grouped = await (from o in context.AssetOwnerships
                    where o.Height >= height
                    group o by o.AssetId
                    into grouping
                    select new
                    {
                        AssetId = grouping.Key,
                        Ownership = grouping.FirstOrDefault(g => g.BalanceQnt > 0)
                    }).ToListAsync();

                return grouped.Select(g => _mapper.Map<AssetOwnership>(g.Ownership));
            }
        }

        public async Task<AssetOwnership> GetAssetOwnershipAtHeightAsync(long nxtAssetId, int height)
        {
            using (var context = new WalletContext())
            {
                var ownership = await context.AssetOwnerships
                    .OrderByDescending(o => o.Height)
                    .FirstOrDefaultAsync(o => o.Asset.NxtId == nxtAssetId && o.Height < height);

                return _mapper.Map<AssetOwnership>(ownership);
            }
        }

        public async Task SaveAssetOwnershipAsync(AssetOwnership assetOwnership)
        {
            var assetOwnershipDto = _mapper.Map<AssetOwnershipDto>(assetOwnership);
            using (var context = new WalletContext())
            {
                context.AssetOwnerships.Add(assetOwnershipDto);
                await context.SaveChangesAsync();
            }
        }

        public async Task SaveAssetOwnershipsAsync(List<AssetOwnership> newOwnerships)
        {
            using (var context = new WalletContext())
            {
                foreach (var ownership in newOwnerships)
                {
                    context.AssetOwnerships.Add(_mapper.Map<AssetOwnershipDto>(ownership));
                }
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdatesAssetOwnershipBalancesAsync(IEnumerable<AssetOwnership> updatedOwnerships)
        {
            using (var context = new WalletContext())
            {
                var existing = (await context.AssetOwnerships
                    .Where(o => updatedOwnerships.Select(ownership => ownership.Id).Contains(o.Id))
                    .ToListAsync()).ToDictionary(o => o.Id);

                foreach (var updatedOwnership in updatedOwnerships)
                {
                    existing[updatedOwnership.Id].BalanceQnt = updatedOwnership.BalanceQnt;
                }

                await context.SaveChangesAsync();
            }
        }
    }
}