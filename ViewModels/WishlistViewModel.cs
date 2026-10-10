using System.Collections.Generic;
using Yash_Gems___Jewelleries.Models;

namespace Yash_Gems___Jewelleries.ViewModels
{
    // Wishlist View Model
    public class WishlistViewModel
    {
        public List<WishlistItemViewModel> Items { get; set; } = new List<WishlistItemViewModel>();
        public decimal TotalPrice => Items.Sum(i => i.Price);
        public int TotalItems => Items.Count;
    }

    // Wishlist Item View Model
    public class WishlistItemViewModel
    {
        public int Id { get; set; }
        public string StyleCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;
        public string MetalType { get; set; } = string.Empty;
        public string StoneType { get; set; } = string.Empty;
        public string Collection { get; set; } = string.Empty;
        public string CollectionImageUrl { get; set; } = string.Empty;
    }
}