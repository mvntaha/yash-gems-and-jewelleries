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
    public class GoldKaratService : IGoldKaratService
    {
        private readonly ApplicationDbContext _context;

        public GoldKaratService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GoldKarat>> GetAllGoldKaratsAsync()
        {
            return await _context.GoldKarats
                .Include(g => g.Items)
                .OrderBy(g => g.GoldCarat)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<GoldKarat?> GetGoldKaratByIdAsync(int id)
        {
            return await _context.GoldKarats
                .Include(g => g.Items)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.GoldTypeId == id);
        }

        public async Task<bool> CreateGoldKaratAsync(GoldKarat karat)
        {
            try
            {
                karat.CreatedDate = DateTime.UtcNow;
                _context.GoldKarats.Add(karat);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateGoldKaratAsync(GoldKarat karat)
        {
            try
            {
                var existing = await _context.GoldKarats.FindAsync(karat.GoldTypeId);
                if (existing == null) return false;

                existing.GoldCarat = karat.GoldCarat;
                existing.PurityPercentage = karat.PurityPercentage;
                existing.Description = karat.Description;
                existing.IsActive = karat.IsActive;
                existing.ModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteGoldKaratAsync(int id)
        {
            try
            {
                var karat = await _context.GoldKarats
                    .Include(g => g.Items)
                    .FirstOrDefaultAsync(g => g.GoldTypeId == id);

                if (karat == null) return false;

                // Soft delete if has related items
                if (karat.Items.Any())
                {
                    karat.IsActive = false;
                    karat.ModifiedDate = DateTime.UtcNow;
                }
                else
                {
                    _context.GoldKarats.Remove(karat);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> GoldCaratExistsAsync(string carat, int? excludeId = null)
        {
            var query = _context.GoldKarats.Where(g => g.GoldCarat == carat);
            
            if (excludeId.HasValue)
            {
                query = query.Where(g => g.GoldTypeId != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
