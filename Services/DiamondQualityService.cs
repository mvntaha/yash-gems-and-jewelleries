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
    public class DiamondQualityService : IDiamondQualityService
    {
        private readonly ApplicationDbContext _context;

        public DiamondQualityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DiamondQuality>> GetAllDiamondQualitiesAsync()
        {
            return await _context.DiamondQualities
                .Include(d => d.DiamondDetails)
                .OrderBy(d => d.QualityGrade)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<DiamondQuality?> GetDiamondQualityByIdAsync(int id)
        {
            return await _context.DiamondQualities
                .Include(d => d.DiamondDetails)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DiamondQualityId == id);
        }

        public async Task<bool> CreateDiamondQualityAsync(DiamondQuality quality)
        {
            try
            {
                quality.CreatedDate = DateTime.UtcNow;
                _context.DiamondQualities.Add(quality);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateDiamondQualityAsync(DiamondQuality quality)
        {
            try
            {
                var existing = await _context.DiamondQualities.FindAsync(quality.DiamondQualityId);
                if (existing == null) return false;

                existing.QualityGrade = quality.QualityGrade;
                existing.Description = quality.Description;
                existing.Clarity = quality.Clarity;
                existing.Color = quality.Color;
                existing.Cut = quality.Cut;
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

        public async Task<bool> DeleteDiamondQualityAsync(int id)
        {
            try
            {
                var quality = await _context.DiamondQualities
                    .Include(d => d.DiamondDetails)
                    .FirstOrDefaultAsync(d => d.DiamondQualityId == id);

                if (quality == null) return false;

                // Soft delete if has related diamond details
                if (quality.DiamondDetails.Any())
                {
                    quality.IsActive = false;
                    quality.ModifiedDate = DateTime.UtcNow;
                }
                else
                {
                    _context.DiamondQualities.Remove(quality);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> QualityGradeExistsAsync(string grade, int? excludeId = null)
        {
            var query = _context.DiamondQualities.Where(d => d.QualityGrade == grade);
            
            if (excludeId.HasValue)
            {
                query = query.Where(d => d.DiamondQualityId != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
