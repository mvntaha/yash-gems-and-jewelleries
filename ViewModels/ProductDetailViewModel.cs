using System.Collections.Generic;
using Yash_Gems___Jewelleries.Models;

namespace Yash_Gems___Jewelleries.ViewModels
{
    public class ProductDetailViewModel
    {
        public Item Item { get; set; } = null!;
        public IEnumerable<Item> RelatedProducts { get; set; } = new List<Item>();
        public IEnumerable<Item> RecentlyViewedItems { get; set; } = new List<Item>();
        public List<string> WishlistStyleCodes { get; set; } = new();
    }
}
