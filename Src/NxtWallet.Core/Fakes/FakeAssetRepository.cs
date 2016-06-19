using System.Collections.Generic;
using System.Threading.Tasks;
using NxtWallet.Core.Models;
using NxtWallet.Repositories.Model;
using System.Linq;

namespace NxtWallet.Core.Fakes
{
    public class FakeAssetRepository : IAssetRepository
    {
        public List<Asset> Assets { get; set; }
        public List<AssetOwnership> AssetOwnerships { get; set; }

        public FakeAssetRepository()
        {
            Assets = new List<Asset>();
            AssetOwnerships = new List<AssetOwnership>();
        }

        public Task<Asset> GetAssetAsync(ulong nxtId)
        {
            var asset = Assets.SingleOrDefault(a => a.NxtId == (long)nxtId);
            return Task.FromResult(asset);
        }

        public Task<IEnumerable<Asset>> GetAssetsAsync(IEnumerable<int> assetIds)
        {
            var assets = Assets.Where(a => assetIds.Contains(a.Id)).ToList();
            return Task.FromResult(assets.AsEnumerable());
        }

        public Task<Asset> SaveAssetAsync(Asset asset)
        {
            var existingAsset = GetAssetAsync((ulong)asset.NxtId).Result;
            if (existingAsset != null)
                return Task.FromResult(existingAsset);

            if (asset.Id == 0)
                asset.Id = Assets.Any() ? Assets.Max(a => a.Id) + 1 : 1;

            Assets.Add(asset);
            return Task.FromResult(asset);
        }

        public Task<IEnumerable<AssetOwnership>> GetAssetOwnershipsAsync(int assetId, int minHeight)
        {
            var ownerships = AssetOwnerships.Where(o => o.AssetId == assetId && o.Height >= minHeight).ToList();
            return Task.FromResult(ownerships.AsEnumerable());
        }

        public Task<IEnumerable<AssetOwnership>> GetOwnershipsOwnedAtHeightAsync(int height)
        {
            var grouped = (from o in AssetOwnerships
                where o.Height < height
                group o by o.AssetId
                into grouping
                select new
                {
                    AssetId = grouping.Key,
                    Ownership = grouping.OrderByDescending(g => g.Height).FirstOrDefault()
                }).ToList();

            var ownerships = grouped.Where(g => g.Ownership.BalanceQnt > 0).Select(g => g.Ownership).ToList();
            return Task.FromResult(ownerships.AsEnumerable());
        }

        public Task<IEnumerable<AssetOwnership>> GetOwnershipsOwnedSinceHeightAsync(int height)
        {
            var grouped = (from o in AssetOwnerships
                where o.Height >= height
                group o by o.AssetId
                into grouping
                select new
                {
                    AssetId = grouping.Key,
                    Ownership = grouping.FirstOrDefault(g => g.BalanceQnt > 0)
                }).ToList();

            var ownerships = grouped.Select(g => g.Ownership);
            return Task.FromResult(ownerships.AsEnumerable());
        }

        public Task<AssetOwnership> GetAssetOwnershipAtHeightAsync(long nxtAssetId, int height)
        {
            AssetOwnership assetOwnership = null;
            var asset = GetAssetAsync((ulong)nxtAssetId).Result;
            if (asset != null)
            {
                assetOwnership = AssetOwnerships.OrderByDescending(o => o.Height)
                    .FirstOrDefault(o => o.AssetId == asset.Id && o.Height < height);
            }
            return Task.FromResult(assetOwnership);
        }

        public Task SaveAssetOwnershipAsync(AssetOwnership assetOwnership)
        {
            if (assetOwnership.Id == 0)
                assetOwnership.Id = AssetOwnerships.Any() ? AssetOwnerships.Max(o => o.Id) + 1 : 1;
            AssetOwnerships.Add(assetOwnership);
            return Task.CompletedTask;
        }

        public async Task SaveAssetOwnershipsAsync(List<AssetOwnership> newOwnerships)
        {
            foreach (var ownewship in newOwnerships)
            {
                await SaveAssetOwnershipAsync(ownewship);
            }
        }

        public Task UpdatesAssetOwnershipBalancesAsync(IEnumerable<AssetOwnership> updatedOwnerships)
        {
            foreach (var updatedOwnership in updatedOwnerships)
            {
                var ownership = AssetOwnerships.Single(o => o.Id == updatedOwnership.Id);
                ownership.BalanceQnt = updatedOwnership.BalanceQnt;
            }

            return Task.CompletedTask;
        }
    }
}
