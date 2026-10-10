using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yash_Gems___Jewelleries.Data;
using Yash_Gems___Jewelleries.Models.Masters;
using Yash_Gems___Jewelleries.Interfaces;

namespace Yash_Gems___Jewelleries.Services
{
    public class ProductTypeService : IProductTypeService
    {
        private readonly ApplicationDbContext _context;

        public ProductTypeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductType>> GetAllProductTypesAsync()
        {
            return await _context.ProductTypes
                .Include(p => p.Items)
                .OrderBy(p => p.ProductTypeName)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ProductType?> GetProductTypeByIdAsync(int id)
        {
            return await _context.ProductTypes
                .Include(p => p.Items)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductTypeId == id);
        }

        public async Task<bool> CreateProductTypeAsync(ProductType productType)
        {
            try
            {
                productType.CreatedDate = DateTime.UtcNow;
                _context.ProductTypes.Add(productType);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateProductTypeAsync(ProductType productType)
        {
            try
            {
                var existing = await _context.ProductTypes.FindAsync(productType.ProductTypeId);
                if (existing == null) return false;

                existing.ProductTypeName = productType.ProductTypeName;
                existing.Description = productType.Description;
                existing.ImageUrl = productType.ImageUrl;
                existing.IsActive = productType.IsActive;
                existing.ModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteProductTypeAsync(int id)
        {
            try
            {
                var productType = await _context.ProductTypes
                    .Include(p => p.Items)
                    .FirstOrDefaultAsync(p => p.ProductTypeId == id);

                if (productType == null) return false;

                // Soft delete if has related items
                if (productType.Items.Any())
                {
                    productType.IsActive = false;
                    productType.ModifiedDate = DateTime.UtcNow;
                }
                else
                {
                    _context.ProductTypes.Remove(productType);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ProductTypeNameExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.ProductTypes.Where(p => p.ProductTypeName == name);
            
            if (excludeId.HasValue)
            {
                query = query.Where(p => p.ProductTypeId != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
