using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Yash_Gems___Jewelleries.Models
{
    /// <summary>
    /// Extended Identity User with custom fields for customer/admin profile
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(500)]
        public string? ProfilePictureUrl { get; set; }

        [StringLength(15)]
        public string? PostalCode { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();
    }
}
