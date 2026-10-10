using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Yash_Gems___Jewelleries.Data;
using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.Models.Enums;
using Yash_Gems___Jewelleries.Models.Masters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Yash_Gems___Jewelleries.Helpers
{
    /// <summary>
    /// Database initializer for seeding additional test data
    /// Use this for development/testing purposes
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Call this method to seed all sample data
        /// Updated to handle Identity Managers and Context
        /// </summary>
        public static async Task SeedAllSampleDataAsync(IServiceProvider services)
        {
            using (var scope = services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

                try
                {
                    logger.LogInformation("Starting database seeding...");

                    await SeedMasterDataAsync(context);
                    await SeedSampleProductsAsync(context);
                    await SeedUsersAsync(userManager, roleManager);
                    await SeedOrdersAsync(context, userManager);
                    await SeedActivitiesAsync(context, userManager);
                    await SeedBannersAsync(context);
                    await SeedDiscountSchemesAsync(context);
                    await SeedNotificationsAsync(context);

                    logger.LogInformation("Database seeding completed successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred during database seeding.");
                    throw;
                }
            }
        }

        public static async Task SeedMasterDataAsync(ApplicationDbContext context)
        {
            // Seed Categories
            var categories = new List<(string Name, string ImageUrl)> 
            { 
                ("Rings", "images/collections/cls-7.jpg"), 
                ("Bracelets", "images/collections/cls-17.jpg"), 
                ("Necklaces", "images/collections/cls-5.jpg"), 
                ("Earrings", "images/collections/cls-12.jpg"), 
                ("Bangles", "images/collections/cls-14.jpg") 
            };

            foreach (var cat in categories)
            {
                if (!await context.Categories.AnyAsync(c => c.CategoryName == cat.Name))
                {
                    context.Categories.Add(new Category 
                    { 
                        CategoryName = cat.Name, 
                        Description = $"{cat.Name} Collection", 
                        Stock = 100,
                        ImageUrl = cat.ImageUrl
                    });
                }
            }
            await context.SaveChangesAsync();

            // Seed Brands
            var brands = new List<(string Name, string LogoUrl)> 
            { 
                ("Yash Gems", "images/brands/yash.png"), 
                ("Tanishq", "images/brands/tanishq.jpg"), 
                ("Malabar Gold", "images/brands/malbar.png"), 
                ("CaratLane", "images/brands/caratlane.webp"), 
                ("Tiffany & Co", "images/brands/tiffanyandco.jpg"), 
                ("Pandora", "images/brands/pandora.jpg") 
            };

            foreach (var brand in brands)
            {
                if (!await context.Brands.AnyAsync(b => b.BrandType == brand.Name))
                {
                    context.Brands.Add(new Brand 
                    { 
                        BrandType = brand.Name, 
                        Description = $"Premium Jewelry by {brand.Name}",
                        LogoUrl = brand.LogoUrl
                    });
                }
            }
            await context.SaveChangesAsync();

            // Seed ProductTypes
            var types = new List<(string Name, string ImageUrl)> 
            { 
                ("Gold Jewelry", "images/products/material/dia-gold.png"), 
                ("Diamond Jewelry", "images/products/material/dia-blue.jpg"), 
                ("Platinum Jewelry", "images/products/material/dia-platanium.png"), 
                ("Silver Jewelry", "images/products/material/dia-white.jpg"), 
                ("Gemstone Jewelry", "images/products/material/dia-pink.jpg") 
            };

            foreach (var type in types)
            {
                if (!await context.ProductTypes.AnyAsync(pt => pt.ProductTypeName == type.Name))
                {
                    context.ProductTypes.Add(new ProductType 
                    { 
                        ProductTypeName = type.Name, 
                        Description = type.Name,
                        ImageUrl = type.ImageUrl
                    });
                }
            }
            await context.SaveChangesAsync();

            // Seed GoldKarats
            var karats = new Dictionary<string, decimal> 
            { 
                { "24K", 99.9m }, 
                { "22K", 91.6m }, 
                { "18K", 75.0m }, 
                { "14K", 58.5m }, 
                { "925 Sterling Silver", 92.5m },
                { "Platinum 950", 95.0m }
            };

            foreach (var k in karats)
            {
                if (!await context.GoldKarats.AnyAsync(g => g.GoldCarat == k.Key))
                {
                    context.GoldKarats.Add(new GoldKarat { GoldCarat = k.Key, PurityPercentage = k.Value, Description = k.Key });
                }
            }
            await context.SaveChangesAsync();

            // Seed Certificates
            var certs = new List<string> { "BIS 916", "IGI", "GIA", "SGL", "None" };
            foreach (var name in certs)
            {
                if (!await context.Certificates.AnyAsync(c => c.CertifyType == name))
                {
                    context.Certificates.Add(new Certificate { CertifyType = name, IssuingAuthority = name });
                }
            }
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Seed sample products for testing
        /// Call this method from Program.cs after role seeding
        /// </summary>
        public static async Task SeedSampleProductsAsync(ApplicationDbContext context)
        {
            // Check if products already exist
            if (await context.Items.AnyAsync())
            {
                return; // Database already has products
            }

            // Fetch Master Data IDs
            var brands = context.Brands.ToList().ToDictionary(b => b.BrandType, b => b.BrandId);
            var categories = context.Categories.ToList().ToDictionary(c => c.CategoryName.ToLower(), c => c.CategoryId);
            var pTypes = context.ProductTypes.ToList().ToDictionary(p => p.ProductTypeName, p => p.ProductTypeId);
            var gKarats = context.GoldKarats.ToList().ToDictionary(k => k.GoldCarat, k => k.GoldTypeId);
            var certs = context.Certificates.ToList().ToDictionary(c => c.CertifyType, c => c.CertificateId);

            // Helpers
            int GetCatId(string name) => categories.TryGetValue(name.ToLower(), out int id) ? id : categories.Values.First();
            int GetBrandId(string name) => brands.TryGetValue(name, out int id) ? id : brands.Values.First();
            int GetTypeId(string name) => pTypes.TryGetValue(name, out int id) ? id : pTypes.Values.First();
            int GetKaratId(string name) => gKarats.TryGetValue(name, out int id) ? id : gKarats.Values.First();
            int GetCertId(string name) => certs.TryGetValue(name, out int id) ? id : certs.Values.First();

            // Sample products from HTML
            var sampleProducts = new List<Item>
            {
                // Product 1: Link Chain Bracelet
                new Item
                {
                    StyleCode = "YGJ-2026-001",
                    ItemName = "Link Chain Bracelet",
                    Description = "Stylish link chain bracelet in white gold.",
                    BrandId = GetBrandId("Malabar Gold"),
                    CategoryId = GetCatId("Bracelet"),
                    ProductTypeId = GetTypeId("Gold Jewelry"),
                    GoldTypeId = GetKaratId("18K"),
                    CertificateId = GetCertId("BIS 916"),
                    Quantity = 15,
                    Pairs = 1,
                    GoldWeight = 12.0m,
                    WastagePercentage = 12m,
                    GoldRate = 6500m,
                    GoldMakingCharges = 3000m,
                    PrimaryImageUrl = "images/products/product-2.jpg",
                    SecondaryImageUrl = "images/products/product-13.jpg",
                    IsActive = true,
                    MRP = 999m,
                    SellingPrice = 891m
                },
                // Product 2: Sparkling Infinity Heart Clasp Snake Chain Bracelet
                new Item
                {
                    StyleCode = "YGJ-2026-002",
                    ItemName = "Sparkling Infinity Heart Clasp Snake Chain Bracelet",
                    Description = "Sterling silver snake chain bracelet with sparkling infinity heart clasp.",
                    BrandId = GetBrandId("CaratLane"),
                    CategoryId = GetCatId("Bracelet"),
                    ProductTypeId = GetTypeId("Silver Jewelry"),
                    GoldTypeId = GetKaratId("925 Sterling Silver"),
                    CertificateId = GetCertId("None"),
                    Quantity = 20,
                    Pairs = 1,
                    GoldWeight = 15.0m,
                    WastagePercentage = 5m,
                    GoldRate = 100m,
                    GoldMakingCharges = 1500m,
                    PrimaryImageUrl = "images/products/product-3.jpg",
                    SecondaryImageUrl = "images/products/product-14.jpg",
                    IsActive = true,
                    IsOnSale = true,
                    MRP = 2899m,
                    SellingPrice = 2499m,
                    DiscountPercentage = 13.79m
                },
                // Product 3: Sparkling Infinity Heart Clasp Snake Chain Bracelet
                new Item
                {
                    StyleCode = "YGJ-2026-003",
                    ItemName = "Sparkling Infinity Heart Clasp Snake Chain Bracelet",
                    Description = "Sterling silver snake chain bracelet (Variant).",
                    BrandId = GetBrandId("CaratLane"),
                    CategoryId = GetCatId("Bracelet"),
                    ProductTypeId = GetTypeId("Silver Jewelry"),
                    GoldTypeId = GetKaratId("925 Sterling Silver"),
                    CertificateId = GetCertId("None"),
                    Quantity = 0, // Out of stock
                    Pairs = 1,
                    GoldWeight = 15.0m,
                    WastagePercentage = 5m,
                    GoldRate = 100m,
                    GoldMakingCharges = 800m,
                    PrimaryImageUrl = "images/products/product-4.jpg",
                    SecondaryImageUrl = "images/products/product-15.jpg",
                    IsActive = true,
                    MRP = 899m,
                    SellingPrice = 773m
                },
                // Product 4: Infinite Lab-Grown Diamond Bangle
                new Item
                {
                    StyleCode = "YGJ-2026-004",
                    ItemName = "Infinite Lab-Grown Diamond Bangle",
                    Description = "Exquisite lab-grown diamond bangle.",
                    BrandId = GetBrandId("Tanishq"),
                    CategoryId = GetCatId("Bracelet"),
                    ProductTypeId = GetTypeId("Diamond Jewelry"),
                    GoldTypeId = GetKaratId("18K"),
                    CertificateId = GetCertId("IGI"),
                    Quantity = 8,
                    Pairs = 1,
                    GoldWeight = 10.0m,
                    WastagePercentage = 10m,
                    GoldRate = 6500m,
                    GoldMakingCharges = 5000m,
                    PrimaryImageUrl = "images/products/product-16.jpg",
                    SecondaryImageUrl = "images/products/product-17.jpg",
                    IsActive = true,
                    MRP = 2599m,
                    SellingPrice = 1847m,
                    DiscountPercentage = 28.9m
                },
                // Product 5: Olive Leaf Band Ring
                new Item
                {
                    StyleCode = "YGJ-2026-005",
                    ItemName = "Olive Leaf Band Ring",
                    Description = "Nature-inspired olive leaf band ring.",
                    BrandId = GetBrandId("Yash Gems"),
                    CategoryId = GetCatId("Ring"),
                    ProductTypeId = GetTypeId("Silver Jewelry"),
                    GoldTypeId = GetKaratId("925 Sterling Silver"),
                    CertificateId = GetCertId("None"),
                    Quantity = 25,
                    Pairs = 1,
                    GoldWeight = 4.0m,
                    WastagePercentage = 5m,
                    GoldRate = 100m,
                    GoldMakingCharges = 500m,
                    PrimaryImageUrl = "images/products/product-18.jpg",
                    SecondaryImageUrl = "images/products/product-19.jpg",
                    IsActive = true,
                    IsOnSale = true,
                    MRP = 899m,
                    SellingPrice = 327m,
                    DiscountPercentage = 63.6m
                },
                // Product 6: Organically Shaped Heart Bangle
                new Item
                {
                    StyleCode = "YGJ-2026-006",
                    ItemName = "Organically Shaped Heart Bangle",
                    Description = "Gold bangle with organically shaped heart motif.",
                    BrandId = GetBrandId("Tanishq"),
                    CategoryId = GetCatId("Bracelet"),
                    ProductTypeId = GetTypeId("Gold Jewelry"),
                    GoldTypeId = GetKaratId("22K"),
                    CertificateId = GetCertId("BIS 916"),
                    Quantity = 5,
                    Pairs = 1,
                    GoldWeight = 35.0m,
                    WastagePercentage = 15m,
                    GoldRate = 6500m,
                    GoldMakingCharges = 4000m,
                    PrimaryImageUrl = "images/products/product-62.jpg",
                    SecondaryImageUrl = "images/products/product-21.jpg",
                    IsActive = true,
                    MRP = 4899m,
                    SellingPrice = 3888m
                },
                // Product 7: Open Heart Bangle
                new Item
                {
                    StyleCode = "YGJ-2026-007",
                    ItemName = "Open Heart Bangle",
                    Description = "Sterling silver open heart bangle.",
                    BrandId = GetBrandId("Yash Gems"),
                    CategoryId = GetCatId("Bracelet"),
                    ProductTypeId = GetTypeId("Silver Jewelry"),
                    GoldTypeId = GetKaratId("925 Sterling Silver"),
                    CertificateId = GetCertId("None"),
                    Quantity = 12,
                    Pairs = 1,
                    GoldWeight = 10.0m,
                    WastagePercentage = 5m,
                    GoldRate = 100m,
                    GoldMakingCharges = 800m,
                    PrimaryImageUrl = "images/products/product-22.jpg",
                    SecondaryImageUrl = "images/products/product-23.jpg",
                    IsActive = true,
                    MRP = 399m,
                    SellingPrice = 214m,
                    DiscountPercentage = 46.3m
                },
                // Product 8: Crystal Birthstone Eternity Circle Charm
                new Item
                {
                    StyleCode = "YGJ-2026-008",
                    ItemName = "Crystal Birthstone Eternity Circle Charm",
                    Description = "White gold charm with crystal birthstone.",
                    BrandId = GetBrandId("Pandora"),
                    CategoryId = GetCatId("Bracelet"),
                    ProductTypeId = GetTypeId("Gold Jewelry"),
                    GoldTypeId = GetKaratId("18K"),
                    CertificateId = GetCertId("None"),
                    Quantity = 18,
                    Pairs = 1,
                    GoldWeight = 2.5m,
                    WastagePercentage = 8m,
                    GoldRate = 6500m,
                    GoldMakingCharges = 2000m,
                    PrimaryImageUrl = "images/products/product-24.jpg",
                    SecondaryImageUrl = "images/products/product-25.jpg",
                    IsActive = true,
                    MRP = 999m,
                    SellingPrice = 865m
                },
                // Product 9: Ball Bracelet
                new Item
                {
                    StyleCode = "YGJ-2026-009",
                    ItemName = "Ball Bracelet",
                    Description = "Sterling silver ball bracelet.",
                    BrandId = GetBrandId("Tiffany & Co"),
                    CategoryId = GetCatId("Bracelet"),
                    ProductTypeId = GetTypeId("Silver Jewelry"),
                    GoldTypeId = GetKaratId("925 Sterling Silver"),
                    CertificateId = GetCertId("None"),
                    Quantity = 10,
                    Pairs = 1,
                    GoldWeight = 18.0m,
                    WastagePercentage = 8m,
                    GoldRate = 100m,
                    GoldMakingCharges = 1500m,
                    PrimaryImageUrl = "images/products/product-26.jpg",
                    SecondaryImageUrl = "images/products/product-27.jpg",
                    IsActive = true,
                    MRP = 2199m,
                    SellingPrice = 1766m
                },
                // Product 10: Engagement Ring in 18k Yellow Gold
                new Item
                {
                    StyleCode = "YGJ-2026-010",
                    ItemName = "Engagement Ring in 18k Yellow Gold",
                    Description = "Classic engagement ring in 18k gold.",
                    BrandId = GetBrandId("Tanishq"),
                    CategoryId = GetCatId("Ring"),
                    ProductTypeId = GetTypeId("Gold Jewelry"),
                    GoldTypeId = GetKaratId("18K"),
                    CertificateId = GetCertId("BIS 916"),
                    Quantity = 8,
                    Pairs = 1,
                    GoldWeight = 4.5m,
                    WastagePercentage = 10m,
                    GoldRate = 5000m,
                    GoldMakingCharges = 2500m,
                    PrimaryImageUrl = "images/products/product-28.jpg",
                    SecondaryImageUrl = "images/products/product-29.jpg",
                    IsActive = true,
                    MRP = 1899m,
                    SellingPrice = 1583m
                },
                // Product 11: Wire Bangle
                new Item
                {
                    StyleCode = "YGJ-2026-011",
                    ItemName = "Wire Bangle",
                    Description = "Simple wire bangle in white gold.",
                    BrandId = GetBrandId("Yash Gems"),
                    CategoryId = GetCatId("Bracelet"),
                    ProductTypeId = GetTypeId("Gold Jewelry"),
                    GoldTypeId = GetKaratId("18K"),
                    CertificateId = GetCertId("None"),
                    Quantity = 30,
                    Pairs = 1,
                    GoldWeight = 3.0m,
                    WastagePercentage = 5m,
                    GoldRate = 5000m,
                    GoldMakingCharges = 1000m,
                    PrimaryImageUrl = "images/products/product-30.jpg",
                    SecondaryImageUrl = "images/products/product-31.jpg",
                    IsActive = true,
                    IsOnSale = true,
                    MRP = 599m,
                    SellingPrice = 142m,
                    DiscountPercentage = 76.2m
                },
                // Product 12: Curb Link Bracelet
                new Item
                {
                    StyleCode = "YGJ-2026-012",
                    ItemName = "Curb Link Bracelet",
                    Description = "Thick curb link bracelet in gold.",
                    BrandId = GetBrandId("Malabar Gold"),
                    CategoryId = GetCatId("Bracelets"),
                    ProductTypeId = GetTypeId("Gold Jewelry"),
                    GoldTypeId = GetKaratId("22K"),
                    CertificateId = GetCertId("BIS 916"),
                    Quantity = 6,
                    Pairs = 1,
                    GoldWeight = 25.0m,
                    WastagePercentage = 12m,
                    GoldRate = 6500m,
                    GoldMakingCharges = 4500m,
                    PrimaryImageUrl = "images/products/product-32.jpg",
                    SecondaryImageUrl = "images/products/product-33.jpg",
                    IsActive = true,
                    MRP = 3599m,
                    SellingPrice = 3210m
                },
                // Product 13: Rouje Lucia Dome Hoop Earrings
                new Item
                {
                    StyleCode = "YGJ-2026-013",
                    ItemName = "Rouje Lucia Dome Hoop Earrings",
                    Description = "Classic dome hoop earrings for a timeless look.",
                    BrandId = GetBrandId("Pandora"),
                    CategoryId = GetCatId("Earrings"),
                    ProductTypeId = GetTypeId("Gold Jewelry"),
                    GoldTypeId = GetKaratId("18K"),
                    CertificateId = GetCertId("None"),
                    Quantity = 12,
                    Pairs = 2,
                    GoldWeight = 12.5m,
                    WastagePercentage = 10m,
                    GoldRate = 6500m,
                    GoldMakingCharges = 2500m,
                    PrimaryImageUrl = "images/products/product-38.jpg",
                    SecondaryImageUrl = "images/products/product-47.jpg",
                    IsActive = true,
                    IsOnSale = true,
                    MRP = 3899m,
                    SellingPrice = 1240m
                },
                // Product 14: Sixteen Stone Narrow Earrings
                new Item
                {
                    StyleCode = "YGJ-2026-014",
                    ItemName = "Sixteen Stone Narrow Earrings",
                    Description = "Exquisite sixteen stone narrow earrings.",
                    BrandId = GetBrandId("Yash Gems"),
                    CategoryId = GetCatId("Earrings"),
                    ProductTypeId = GetTypeId("Diamond Jewelry"),
                    GoldTypeId = GetKaratId("18K"),
                    CertificateId = GetCertId("IGI"),
                    Quantity = 7,
                    Pairs = 2,
                    GoldWeight = 8.2m,
                    WastagePercentage = 12m,
                    GoldRate = 6500m,
                    GoldMakingCharges = 6000m,
                    PrimaryImageUrl = "images/products/product-41.jpg",
                    SecondaryImageUrl = "images/products/product-42.jpg",
                    IsActive = true,
                    IsOnSale = true,
                    MRP = 3899m,
                    SellingPrice = 1990m
                },
                // Product 15: Sparkling Moon & Star Stud Earrings
                new Item
                {
                    StyleCode = "YGJ-2026-015",
                    ItemName = "Sparkling Moon & Star Stud Earrings",
                    Description = "Whimsical moon and star stud earrings.",
                    BrandId = GetBrandId("CaratLane"),
                    CategoryId = GetCatId("Earrings"),
                    ProductTypeId = GetTypeId("Silver Jewelry"),
                    GoldTypeId = GetKaratId("925 Sterling Silver"),
                    CertificateId = GetCertId("None"),
                    Quantity = 15,
                    Pairs = 2,
                    GoldWeight = 5.0m,
                    WastagePercentage = 8m,
                    GoldRate = 100m,
                    GoldMakingCharges = 1200m,
                    PrimaryImageUrl = "images/products/product-39.jpg",
                    SecondaryImageUrl = "images/products/product-40.jpg",
                    IsActive = true,
                    IsOnSale = true,
                    MRP = 3899m,
                    SellingPrice = 2990m
                },
                // Product 16: Oval Yellow Diamond Double Halo Engagement Ring
                new Item
                {
                    StyleCode = "YGJ-2026-016",
                    ItemName = "Oval Yellow Diamond Double Halo Engagement Ring",
                    Description = "Premium variant of the oval yellow diamond ring.",
                    BrandId = GetBrandId("Tiffany & Co"),
                    CategoryId = GetCatId("Rings"),
                    ProductTypeId = GetTypeId("Diamond Jewelry"),
                    GoldTypeId = GetKaratId("18K"),
                    CertificateId = GetCertId("GIA"),
                    Quantity = 3,
                    Pairs = 1,
                    GoldWeight = 7.5m,
                    WastagePercentage = 15m,
                    GoldRate = 6500m,
                    GoldMakingCharges = 9000m,
                    PrimaryImageUrl = "images/products/product-54.jpg",
                    SecondaryImageUrl = "images/products/product-55.jpg",
                    IsActive = true,
                    IsNewLaunch = true,
                    MRP = 2499m,
                    SellingPrice = 2499m
                },
                // Product 17: Small Earrings In Gold with Diamond
                new Item
                {
                    StyleCode = "YGJ-2026-017",
                    ItemName = "Small Earrings In Gold with Diamond",
                    Description = "Elegant small gold earrings with a single diamond.",
                    BrandId = GetBrandId("Malabar Gold"),
                    CategoryId = GetCatId("Earrings"),
                    ProductTypeId = GetTypeId("Diamond Jewelry"),
                    GoldTypeId = GetKaratId("22K"),
                    CertificateId = GetCertId("BIS 916"),
                    Quantity = 10,
                    Pairs = 2,
                    GoldWeight = 4.2m,
                    WastagePercentage = 10m,
                    GoldRate = 6500m,
                    GoldMakingCharges = 3500m,
                    PrimaryImageUrl = "images/products/product-56.jpg",
                    SecondaryImageUrl = "images/products/product-57.jpg",
                    IsActive = true,
                    IsOnSale = true,
                    MRP = 3899m,
                    SellingPrice = 3370m
                },
                // Product 18: Sixteen Stone Narrow Earrings
                new Item
                {
                    StyleCode = "YGJ-2026-018",
                    ItemName = "Sixteen Stone Narrow Earrings",
                    Description = "Special offer on sixteen stone narrow earrings.",
                    BrandId = GetBrandId("Yash Gems"),
                    CategoryId = GetCatId("Earrings"),
                    ProductTypeId = GetTypeId("Diamond Jewelry"),
                    GoldTypeId = GetKaratId("18K"),
                    CertificateId = GetCertId("IGI"),
                    Quantity = 5,
                    Pairs = 2,
                    GoldWeight = 8.2m,
                    WastagePercentage = 12m,
                    GoldRate = 6500m,
                    GoldMakingCharges = 5500m,
                    PrimaryImageUrl = "images/products/product-58.jpg",
                    SecondaryImageUrl = "images/products/product-59.jpg",
                    IsActive = true,
                    IsOnSale = true,
                    MRP = 2899m,
                    SellingPrice = 2499m
                },
                // Product 19: Sparkling Moon & Star Stud Earrings (Best Sellers - Out of Stock)
                new Item
                {
                    StyleCode = "YGJ-2026-019",
                    ItemName = "Sparkling Moon & Star Stud Earrings (Notify Me)",
                    Description = "Currently unavailable moon and star earrings.",
                    BrandId = GetBrandId("CaratLane"),
                    CategoryId = GetCatId("Earrings"), // Use GetCatId directly
                    ProductTypeId = GetTypeId("Silver Jewelry"),
                    GoldTypeId = GetKaratId("925 Sterling Silver"),
                    CertificateId = GetCertId("None"),
                    Quantity = 0,
                    Pairs = 2,
                    GoldWeight = 5.0m,
                    WastagePercentage = 8m,
                    GoldRate = 100m,
                    GoldMakingCharges = 1200m,
                    PrimaryImageUrl = "images/products/product-4.jpg",
                    SecondaryImageUrl = "images/products/product-13.jpg",
                    IsActive = true,
                    MRP = 2499m,
                    SellingPrice = 2499m
                },
                // Product 20: Emerald-cut Halo Engagement Ring
                new Item
                {
                    StyleCode = "YGJ-2026-020",
                    ItemName = "Emerald-cut Halo Engagement Ring",
                    Description = "Bestselling emerald-cut halo engagement ring.",
                    BrandId = GetBrandId("Tiffany & Co"),
                    CategoryId = GetCatId("Rings"),
                    ProductTypeId = GetTypeId("Platinum Jewelry"),
                    GoldTypeId = GetKaratId("Platinum 950"),
                    CertificateId = GetCertId("GIA"),
                    Quantity = 4,
                    Pairs = 1,
                    GoldWeight = 5.5m,
                    WastagePercentage = 10m,
                    GoldRate = 6500m,
                    GoldMakingCharges = 2000m,
                    PrimaryImageUrl = "images/products/product-39.jpg",
                    SecondaryImageUrl = "images/products/product-40.jpg",
                    IsActive = true,
                    IsFeatured = true,
                    MRP = 3899m,
                    SellingPrice = 3370m
                }
            };


            foreach (var product in sampleProducts)
            {
                // Calculate internal fields (wastage, net weight) but do not overwrite MRP/SP via CalculatePricing
                product.WastageAmount = (product.GoldWeight * product.WastagePercentage) / 100;
                product.NetGoldWeight = product.GoldWeight + product.WastageAmount;
                product.GoldAmount = product.NetGoldWeight * product.GoldRate;
                
                // Set default discount percentage if IsOnSale but no discount set?
                // Or if MRP > SellingPrice, calculate it
                if(product.MRP > product.SellingPrice && product.DiscountPercentage == 0)
                {
                   product.DiscountPercentage = ((product.MRP - product.SellingPrice) / product.MRP) * 100;
                   product.DiscountPercentage = Math.Round(product.DiscountPercentage, 2);
                }
            }

            await context.Items.AddRangeAsync(sampleProducts);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Seed sample banners for homepage
        /// </summary>
        public static async Task SeedBannersAsync(ApplicationDbContext context)
        {
            if (await context.Banners.AnyAsync())
            {
                return; // Banners already exist
            }

            var banners = new List<Banner>
            {
                new Banner
                {
                    Title = "Radiate Elegance, Wear Confidence",
                    Description = "Discover our latest jewelry designs",
                    ImageUrl = "/app/images/banner/banner-10.jpg",
                    LinkUrl = "/Item",
                    ButtonText = "Shop Collection",
                    DisplayOrder = 1,
                    BannerType = BannerType.NewIn,
                    IsActive = true
                },
                new Banner
                {
                    Title = "Unveil Your Sparkle",
                    Description = "Discover our handcrafted jewelry collection designed to elevate your style. Enjoy exclusive deals and limited-time offers—your moment to shine is now!",
                    ImageUrl = "/app/images/banner/banner-4.jpg",
                    LinkUrl = "/Item",
                    ButtonText = "Shop Now",
                    DisplayOrder = 2,
                    BannerType = BannerType.Promotional,
                    IsActive = true
                }
            };

            await context.Banners.AddRangeAsync(banners);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Seed sample discount schemes
        /// </summary>
        public static async Task SeedDiscountSchemesAsync(ApplicationDbContext context)
        {
            if (await context.DiscountSchemes.AnyAsync())
            {
                return; // Schemes already exist
            }

            var schemes = new List<DiscountScheme>
            {
                new DiscountScheme
                {
                    SchemeName = "New Year Offer",
                    Description = "Flat 20% off on all gold jewelry",
                    DiscountType = "Percentage",
                    DiscountValue = 20m,
                    MinimumPurchaseAmount = 50000m,
                    MaximumDiscountAmount = 15000m,
                    CouponCode = "NEWYEAR2026",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(1),
                    ApplicableOn = "Category",
                    ApplicableIds = "1", // Gold category
                    IsActive = true,
                    IsFeatured = true
                },
                new DiscountScheme
                {
                    SchemeName = "First Purchase Discount",
                    Description = "$5000 off on your first order",
                    DiscountType = "FixedAmount",
                    DiscountValue = 5000m,
                    MinimumPurchaseAmount = 25000m,
                    CouponCode = "FIRST5000",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(6),
                    PerUserLimit = 1,
                    ApplicableOn = "All",
                    IsActive = true
                }
            };

            await context.DiscountSchemes.AddRangeAsync(schemes);
            await context.SaveChangesAsync();
        }

        public static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var users = new List<(string Email, string FirstName, string LastName, string Phone, string Address, string City, string State, string Zip)>
            {
                ("john.doe@example.com", "John", "Doe", "+1234567890", "123 Maple Street", "Karachi", "Sindh", "74000"),
                ("jane.smith@example.com", "Jane", "Smith", "+1234567891", "456 Oak Avenue", "Lahore", "Punjab", "54000"),
                ("mike.brown@example.com", "Mike", "Brown", "+1234567892", "789 Pine Road", "Islamabad", "ICT", "44000")
            };

            foreach (var u in users)
            {
                if (await userManager.FindByEmailAsync(u.Email) == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = u.Email,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        PhoneNumber = u.Phone,
                        Address = u.Address,
                        City = u.City,
                        State = u.State,
                        PostalCode = u.Zip,
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow.AddMonths(-1)
                    };

                    var result = await userManager.CreateAsync(user, "User@12345");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Customer");
                    }
                }
            }
        }

        public static async Task SeedOrdersAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            if (await context.Orders.AnyAsync()) return;

            var customers = await userManager.GetUsersInRoleAsync("Customer");
            var products = await context.Items.Take(10).ToListAsync();

            if (!customers.Any() || !products.Any()) return;

            var random = new Random();

            foreach (var customer in customers)
            {
                // Create 1-2 orders per customer
                int orderCount = random.Next(1, 3);
                for (int i = 0; i < orderCount; i++)
                {
                    var seededOrder = new Order
                    {
                        OrderNumber = Order.GenerateOrderNumber(),
                        UserId = customer.Id,
                        OrderDate = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                        CustomerName = customer.FirstName + " " + customer.LastName,
                        Email = customer.Email ?? "customer@example.com",
                        PhoneNumber = customer.PhoneNumber ?? "1234567890",
                        ShippingAddress = customer.Address ?? "123 Main St",
                        ShippingCity = customer.City ?? "Karachi",
                        ShippingState = customer.State ?? "Sindh",
                        ShippingPostalCode = customer.PostalCode ?? "74000",
                        PaymentMethod = "COD",
                        PaymentStatus = "Pending",
                        OrderStatus = OrderStatus.Pending,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow,
                        OrderItems = new List<OrderItem>()
                    };

                    int itemCount = random.Next(1, 4);
                    decimal subtotal = 0;

                    for (int j = 0; j < itemCount; j++)
                    {
                        var product = products[random.Next(products.Count)];
                        var orderItem = new OrderItem
                        {
                            StyleCode = product.StyleCode,
                            ProductName = product.ItemName,
                            Quantity = random.Next(1, 3),
                            UnitPrice = product.SellingPrice,
                            DiscountPercentage = product.DiscountPercentage,
                            ProductImageUrl = product.PrimaryImageUrl
                        };
                        orderItem.CalculateSubtotal();
                        subtotal += orderItem.Subtotal;
                        seededOrder.OrderItems.Add(orderItem);
                    }

                    seededOrder.Subtotal = subtotal;
                    seededOrder.TaxAmount = 3.00m; 
                    seededOrder.ShippingCharges = 10.00m;
                    seededOrder.CalculateTotalAmount();

                    context.Orders.Add(seededOrder);

                    // Add status history
                    context.StatusHistories.Add(new OrderStatusHistory
                    {
                        Order = seededOrder,
                        Status = OrderStatus.Pending,
                        CreatedAt = DateTime.UtcNow,
                        Notes = "Order placed"
                    });
                }
            }
            await context.SaveChangesAsync();
        }

        public static async Task SeedActivitiesAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            var customers = await userManager.GetUsersInRoleAsync("Customer");
            var products = await context.Items.Take(5).ToListAsync();
            var random = new Random();

            foreach (var customer in customers)
            {
                // Seed CartItems
                if (!await context.CartItems.AnyAsync(c => c.UserId == customer.Id))
                {
                    var product = products[random.Next(products.Count)];
                    var cartItem = new CartItem
                    {
                        UserId = customer.Id,
                        StyleCode = product.StyleCode,
                        Quantity = random.Next(1, 3),
                        PriceAtAdd = product.SellingPrice,
                        AddedDate = DateTime.UtcNow.AddHours(-random.Next(1, 24))
                    };
                    cartItem.CalculateSubtotal();
                    context.CartItems.Add(cartItem);
                }

                // Seed Wishlist
                if (!await context.Wishlists.AnyAsync(w => w.UserId == customer.Id))
                {
                    var product = products[random.Next(products.Count)];
                    context.Wishlists.Add(new Wishlist
                    {
                        UserId = customer.Id,
                        StyleCode = product.StyleCode,
                        AddedDate = DateTime.UtcNow.AddDays(-random.Next(1, 10)),
                        Notes = "I love this design!"
                    });
                }

                // Seed Inquiries
                if (!await context.Inquiries.AnyAsync(i => i.UserId == customer.Id))
                {
                    var product = products[random.Next(products.Count)];
                    context.Inquiries.Add(new Inquiry
                    {
                        UserId = customer.Id,
                        StyleCode = product.StyleCode,
                        FullName = $"{customer.FirstName} {customer.LastName}",
                        Email = customer.Email!,
                        PhoneNumber = customer.PhoneNumber ?? "+1234567890",
                        InquiryType = "Product",
                        Subject = "Query about " + product.ItemName,
                        Message = "Could you please provide more details about the certification for this product?",
                        CreatedDate = DateTime.UtcNow.AddDays(-random.Next(1, 5)),
                        Status = "New"
                    });
                }
            }
            await context.SaveChangesAsync();
        }

        public static async Task SeedNotificationsAsync(ApplicationDbContext context)
        {
            if (await context.Notifications.AnyAsync()) return;

            var notifications = new List<Notification>
            {
                new Notification
                {
                    Title = "System Update",
                    Message = "The inventory management system has been updated to v2.1.",
                    Type = NotificationType.OrderStatusUpdate,
                    TargetRole = "Admin",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new Notification
                {
                    Title = "Low Stock Alert",
                    Message = "Product YGJ-2026-004 is almost out of stock.",
                    Type = NotificationType.LowInventoryThreshold,
                    ReferenceId = "YGJ-2026-004",
                    TargetRole = "Admin",
                    CreatedAt = DateTime.UtcNow.AddHours(-5)
                },
                new Notification
                {
                    Title = "Welcome to Yash Gems",
                    Message = "Thank you for registering! Explore our latest collections.",
                    Type = NotificationType.NewCustomerRegistration,
                    TargetRole = "Customer",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            await context.Notifications.AddRangeAsync(notifications);
            await context.SaveChangesAsync();
        }
    }
}
