using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Data.Entity;
using NxtWallet.ViewModel.Model;

namespace NxtWallet.Model
{
    public interface IAssetRepository
    {
        Task<Asset> GetAssetAsync(ulong nxtId);
        Task<Asset> SaveAssetAsync(Asset asset);
        Task<IEnumerable<AssetOwnership>> GetAssetOwnershipsAsync(int assetId, int minHeight);
        Task<AssetOwnership> GetAssetOwnershipsAtHeightAsync(long nxtAssetId, int height);
        Task SaveAssetOwnershipAsync(AssetOwnership assetOwnership);
        Task SaveAssetOwnershipsAsync(List<AssetOwnership> newOwnerships);
        Task UpdatesAssetOwnershipsAsync(List<AssetOwnership> updatedOwnerships);
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

        public async Task<AssetOwnership> GetAssetOwnershipsAtHeightAsync(long nxtAssetId, int height)
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

        public async Task UpdatesAssetOwnershipsAsync(List<AssetOwnership> updatedOwnerships)
        {
            using (var context = new WalletContext())
            {
                foreach (var dto in updatedOwnerships.Select(ownership => _mapper.Map<AssetOwnershipDto>(ownership)))
                {
                    context.AssetOwnerships.Attach(dto);
                    context.Entry(dto).State = EntityState.Modified;
                }
                await context.SaveChangesAsync();
            }
        }
    }
}