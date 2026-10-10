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
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.Items)
                .OrderBy(c => c.CategoryName)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Items)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryId == id);
        }

        public async Task<bool> CreateCategoryAsync(Category category)
        {
            try
            {
                category.CreatedDate = DateTime.UtcNow;
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            try
            {
                var existing = await _context.Categories.FindAsync(category.CategoryId);
                if (existing == null) return false;

                existing.CategoryName = category.CategoryName;
                existing.Description = category.Description;
                existing.ImageUrl = category.ImageUrl;
                existing.Stock = category.Stock;
                existing.IsActive = category.IsActive;
                existing.ModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.CategoryId == id);

                if (category == null) return false;

                // Soft delete if has related items
                if (category.Items.Any())
                {
                    category.IsActive = false;
                    category.ModifiedDate = DateTime.UtcNow;
                }
                else
                {
                    _context.Categories.Remove(category);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CategoryNameExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.Categories.Where(c => c.CategoryName == name);
            
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.CategoryId != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
