using Yash_Gems___Jewelleries.Models;

namespace Yash_Gems___Jewelleries.Interfaces
{
    public interface IDiscountService
    {
        bool IsSchemeValid(DiscountScheme? scheme);
        decimal CalculateDiscountedPrice(decimal originalPrice, DiscountScheme scheme);
        string GetDiscountMessage(DiscountScheme scheme);
        Task<List<DiscountScheme>> GetActiveSchemesAsync();
    }
}
