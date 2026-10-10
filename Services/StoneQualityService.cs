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
    public class StoneQualityService : IStoneQualityService
    {
        private readonly ApplicationDbContext _context;

        public StoneQualityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StoneQuality>> GetAllStoneQualitiesAsync()
        {
            return await _context.StoneQualities
                .Include(s => s.StoneDetails)
                .OrderBy(s => s.StoneType)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<StoneQuality?> GetStoneQualityByIdAsync(int id)
        {
            return await _context.StoneQualities
                .Include(s => s.StoneDetails)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StoneQualityId == id);
        }

        public async Task<bool> CreateStoneQualityAsync(StoneQuality quality)
        {
            try
            {
                quality.CreatedDate = DateTime.UtcNow;
                _context.StoneQualities.Add(quality);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateStoneQualityAsync(StoneQuality quality)
        {
            try
            {
                var existing = await _context.StoneQualities.FindAsync(quality.StoneQualityId);
                if (existing == null) return false;

                existing.StoneType = quality.StoneType;
                existing.QualityGrade = quality.QualityGrade;
                existing.Description = quality.Description;
                existing.Color = quality.Color;
                existing.Origin = quality.Origin;
                existing.IsActive = quality.IsActive;
                existing.ModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteStoneQualityAsync(int id)
        {
            try
            {
                var quality = await _context.StoneQualities
                    .Include(s => s.StoneDetails)
                    .FirstOrDefaultAsync(s => s.StoneQualityId == id);

                if (quality == null) return false;

                // Soft delete if has related stone details
                if (quality.StoneDetails.Any())
                {
                    quality.IsActive = false;
                    quality.ModifiedDate = DateTime.UtcNow;
                }
                else
                {
                    _context.StoneQualities.Remove(quality);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> StoneTypeExistsAsync(string type, int? excludeId = null)
        {
            var query = _context.StoneQualities.Where(s => s.StoneType == type);
            
            if (excludeId.HasValue)
            {
                query = query.Where(s => s.StoneQualityId != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
