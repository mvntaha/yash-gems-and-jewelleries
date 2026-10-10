using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yash_Gems___Jewelleries.Data;
using Yash_Gems___Jewelleries.Models.Masters;
using Yash_Gems___Jewelleries.Interfaces;

namespace Yash_Gems___Jewelleries.Services
{
    public class BrandService : IBrandService
    {
        private readonly ApplicationDbContext _context;

        public BrandService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Brand>> GetAllBrandsAsync(bool? isActive = null)
        {
            var query = _context.Brands
                .Include(b => b.Items)
                .AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(b => b.IsActive == isActive.Value);
            }

            return await query
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();
        }

        public async Task<Brand?> GetBrandByIdAsync(int id)
        {
            return await _context.Brands.FindAsync(id);
        }

        public async Task<bool> CreateBrandAsync(Brand brand)
        {
            try
            {
                brand.CreatedDate = DateTime.UtcNow;
                _context.Brands.Add(brand);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateBrandAsync(Brand brand)
        {
            try
            {
                var existingBrand = await _context.Brands.FindAsync(brand.BrandId);
                if (existingBrand == null) return false;

                existingBrand.BrandType = brand.BrandType;
                existingBrand.Description = brand.Description;
                existingBrand.LogoUrl = brand.LogoUrl;
                existingBrand.Website = brand.Website;
                existingBrand.Location = brand.Location;
                existingBrand.Email = brand.Email;
                existingBrand.PhoneNumber = brand.PhoneNumber;
                existingBrand.IsActive = brand.IsActive;
                existingBrand.ModifiedDate = DateTime.UtcNow;

                _context.Brands.Update(existingBrand);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteBrandAsync(int id)
        {
            try
            {
                var brand = await _context.Brands.FindAsync(id);
                if (brand == null) return false;

                _context.Brands.Remove(brand);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> BrandExistsAsync(int id)
        {
            return await _context.Brands.AnyAsync(b => b.BrandId == id);
        }

        public async Task<bool> BrandNameExistsAsync(string name, int? excludeId = null)
        {
            return await _context.Brands.AnyAsync(b => b.BrandType.ToLower() == name.ToLower() && (!excludeId.HasValue || b.BrandId != excludeId.Value));
        }
    }
}
