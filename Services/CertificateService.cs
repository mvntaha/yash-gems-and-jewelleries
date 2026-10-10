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
    public class CertificateService : ICertificateService
    {
        private readonly ApplicationDbContext _context;

        public CertificateService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Certificate>> GetAllCertificatesAsync()
        {
            return await _context.Certificates
                .Include(c => c.Items)
                .OrderBy(c => c.CertifyType)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Certificate?> GetCertificateByIdAsync(int id)
        {
            return await _context.Certificates
                .Include(c => c.Items)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CertificateId == id);
        }

        public async Task<bool> CreateCertificateAsync(Certificate certificate)
        {
            try
            {
                certificate.CreatedDate = DateTime.UtcNow;
                _context.Certificates.Add(certificate);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCertificateAsync(Certificate certificate)
        {
            try
            {
                var existing = await _context.Certificates.FindAsync(certificate.CertificateId);
                if (existing == null) return false;

                existing.CertifyType = certificate.CertifyType;
                existing.Description = certificate.Description;
                existing.IssuingAuthority = certificate.IssuingAuthority;
                existing.IsActive = certificate.IsActive;
                existing.ModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCertificateAsync(int id)
        {
            try
            {
                var certificate = await _context.Certificates
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.CertificateId == id);

                if (certificate == null) return false;

                // Soft delete if has related items
                if (certificate.Items.Any())
                {
                    certificate.IsActive = false;
                    certificate.ModifiedDate = DateTime.UtcNow;
                }
                else
                {
                    _context.Certificates.Remove(certificate);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CertificateTypeExistsAsync(string type, int? excludeId = null)
        {
            var query = _context.Certificates.Where(c => c.CertifyType == type);
            
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.CertificateId != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
