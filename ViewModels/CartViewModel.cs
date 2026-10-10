using System.ComponentModel.DataAnnotations;

namespace Yash_Gems___Jewelleries.ViewModels
{
    // Cart View Model
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public List<Yash_Gems___Jewelleries.Models.Item> RecommendedProducts { get; set; } = new List<Yash_Gems___Jewelleries.Models.Item>();
        public List<string> WishlistStyleCodes { get; set; } = new List<string>();
        
        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }
        
        [Display(Name = "Shipping")]
        public decimal ShippingCost { get; set; } = 0;
        
        [Display(Name = "Tax")]
        public decimal Tax { get; set; } = 0;

        public decimal TaxRate { get; set; } = 0;
        
        [Display(Name = "Total")]
        public decimal Total { get; set; }
    }

    // Cart Item View Model
    public class CartItemViewModel
    {
        public int CartItemId { get; set; }
        public string StyleCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public int MaxStock { get; set; }
    }
}
