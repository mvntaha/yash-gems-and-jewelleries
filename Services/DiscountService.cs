using Microsoft.EntityFrameworkCore;
using Yash_Gems___Jewelleries.Data;
using Yash_Gems___Jewelleries.Interfaces;
using Yash_Gems___Jewelleries.Models;

namespace Yash_Gems___Jewelleries.Services
{
    public class DiscountService : IDiscountService
    {
        private readonly ApplicationDbContext _context;

        public DiscountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool IsSchemeValid(DiscountScheme? scheme)
        {
            if (scheme == null) return false;

            return scheme.IsValid();
        }

        public decimal CalculateDiscountedPrice(decimal originalPrice, DiscountScheme scheme)
        {
            if (!IsSchemeValid(scheme)) return originalPrice;

            decimal discountAmount = 0;

            if (scheme.DiscountType == "Percentage")
            {
                discountAmount = originalPrice * (scheme.DiscountValue / 100);
            }
            else if (scheme.DiscountType == "FixedAmount")
            {
                discountAmount = scheme.DiscountValue;
            }

            // Apply max discount limit if set
            if (scheme.MaximumDiscountAmount.HasValue && discountAmount > scheme.MaximumDiscountAmount.Value)
            {
                discountAmount = scheme.MaximumDiscountAmount.Value;
            }

            decimal finalPrice = originalPrice - discountAmount;
            return Math.Max(0, finalPrice);
        }

        public string GetDiscountMessage(DiscountScheme scheme)
        {
            if (scheme.DiscountType == "Percentage")
            {
                return $"GET {scheme.DiscountValue:G29}% OFF";
            }
            else
            {
                return $"SAVE {scheme.DiscountValue:C}";
            }
        }

        public async Task<List<DiscountScheme>> GetActiveSchemesAsync()
        {
            DateTime now = DateTime.UtcNow;
            return await _context.DiscountSchemes
                .Where(s => s.IsActive && now >= s.StartDate && now <= s.EndDate)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();
        }
    }
}
