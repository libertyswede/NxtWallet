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
        Task SaveAssetOwnershipAsync(AssetOwnership assetOwnership);
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

        public async Task SaveAssetOwnershipAsync(AssetOwnership assetOwnership)
        {
            var assetOwnershipDto = _mapper.Map<AssetOwnershipDto>(assetOwnership);
            using (var context = new WalletContext())
            {
                context.AssetOwnerships.Add(assetOwnershipDto);
                await context.SaveChangesAsync();
            }
        }
    }
}