using System.Collections.Generic;
using System.Threading.Tasks;
using Yash_Gems___Jewelleries.Models.Masters;
using Yash_Gems___Jewelleries.Repositories;
using Yash_Gems___Jewelleries.Interfaces;

namespace Yash_Gems___Jewelleries.Services
{
    public class MasterLookupService : IMasterLookupService
    {
        private readonly IMasterDataRepository<Brand> _brandRepo;
        private readonly IMasterDataRepository<Category> _categoryRepo;
        private readonly IMasterDataRepository<ProductType> _productTypeRepo;
        private readonly IMasterDataRepository<GoldKarat> _goldKaratRepo;
        private readonly IMasterDataRepository<Certificate> _certificateRepo;
        private readonly IMasterDataRepository<DiamondQuality> _diamondQualityRepo;
        private readonly IMasterDataRepository<StoneQuality> _stoneQualityRepo;

        public MasterLookupService(
            IMasterDataRepository<Brand> brandRepo,
            IMasterDataRepository<Category> categoryRepo,
            IMasterDataRepository<ProductType> productTypeRepo,
            IMasterDataRepository<GoldKarat> goldKaratRepo,
            IMasterDataRepository<Certificate> certificateRepo,
            IMasterDataRepository<DiamondQuality> diamondQualityRepo,
            IMasterDataRepository<StoneQuality> stoneQualityRepo)
        {
            _brandRepo = brandRepo;
            _categoryRepo = categoryRepo;
            _productTypeRepo = productTypeRepo;
            _goldKaratRepo = goldKaratRepo;
            _certificateRepo = certificateRepo;
            _diamondQualityRepo = diamondQualityRepo;
            _stoneQualityRepo = stoneQualityRepo;
        }

        public async Task<IEnumerable<Brand>> GetActiveBrandsAsync() => await _brandRepo.GetAllAsync(true);
        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync() => await _categoryRepo.GetAllAsync(true);
        public async Task<IEnumerable<ProductType>> GetActiveProductTypesAsync() => await _productTypeRepo.GetAllAsync(true);
        public async Task<IEnumerable<GoldKarat>> GetActiveGoldKaratsAsync() => await _goldKaratRepo.GetAllAsync(true);
        public async Task<IEnumerable<Certificate>> GetActiveCertificatesAsync() => await _certificateRepo.GetAllAsync(true);
        public async Task<IEnumerable<DiamondQuality>> GetActiveDiamondQualitiesAsync() => await _diamondQualityRepo.GetAllAsync(true);
        public async Task<IEnumerable<StoneQuality>> GetActiveStoneQualitiesAsync() => await _stoneQualityRepo.GetAllAsync(true);

        public async Task<bool> IsBrandValidAsync(int id) => await _brandRepo.ExistsAsync(id);
        public async Task<bool> IsCategoryValidAsync(int id) => await _categoryRepo.ExistsAsync(id);
        public async Task<bool> IsProductTypeValidAsync(int id) => await _productTypeRepo.ExistsAsync(id);
        public async Task<bool> IsGoldKaratValidAsync(int id) => await _goldKaratRepo.ExistsAsync(id);
        public async Task<bool> IsCertificateValidAsync(int? id) => !id.HasValue || await _certificateRepo.ExistsAsync(id.Value);
        public async Task<bool> IsDiamondQualityValidAsync(int id) => await _diamondQualityRepo.ExistsAsync(id);
        public async Task<bool> IsStoneQualityValidAsync(int id) => await _stoneQualityRepo.ExistsAsync(id);
    }
}
