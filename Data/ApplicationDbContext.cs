using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Yash_Gems___Jewelleries.Models.Masters;
using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.Models.Enums;

namespace Yash_Gems___Jewelleries.Data
{
    /// <summary>
    /// Database context for Yash Gems & Jewelleries e-commerce system
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Master Tables
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<GoldKarat> GoldKarats { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }
        public DbSet<DiamondQuality> DiamondQualities { get; set; }
        public DbSet<StoneQuality> StoneQualities { get; set; }

        // Main Tables
        public DbSet<Item> Items { get; set; }
        public DbSet<DiamondDetail> DiamondDetails { get; set; }
        public DbSet<StoneDetail> StoneDetails { get; set; }

        // Shopping & Orders
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderStatusHistory> StatusHistories { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }

        // Customer Interaction
        public DbSet<Inquiry> Inquiries { get; set; }

        // Marketing & Promotions
        public DbSet<Banner> Banners { get; set; }
        public DbSet<DiscountScheme> DiscountSchemes { get; set; }
        public DbSet<NewsletterSubscription> NewsletterSubscriptions { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Item primary key
            modelBuilder.Entity<Item>()
                .HasKey(i => i.StyleCode);

            // Configure relationships and delete behaviors
            ConfigureRelationships(modelBuilder);

            // Configure indexes for performance
            ConfigureIndexes(modelBuilder);

            // Configure decimal precision
            ConfigureDecimalPrecision(modelBuilder);

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            // Item relationships
            modelBuilder.Entity<Item>()
                .HasOne(i => i.Brand)
                .WithMany(b => b.Items)
                .HasForeignKey(i => i.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.Certificate)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.CertificateId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.ProductType)
                .WithMany(p => p.Items)
                .HasForeignKey(i => i.ProductTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.GoldKarat)
                .WithMany(g => g.Items)
                .HasForeignKey(i => i.GoldTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // DiamondDetail relationships
            modelBuilder.Entity<DiamondDetail>()
                .HasOne(d => d.Item)
                .WithMany(i => i.DiamondDetails)
                .HasForeignKey(d => d.StyleCode)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DiamondDetail>()
                .HasOne(d => d.DiamondQuality)
                .WithMany(dq => dq.DiamondDetails)
                .HasForeignKey(d => d.DiamondQualityId)
                .OnDelete(DeleteBehavior.Restrict);

            // StoneDetail relationships
            modelBuilder.Entity<StoneDetail>()
                .HasOne(s => s.Item)
                .WithMany(i => i.StoneDetails)
                .HasForeignKey(s => s.StyleCode)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StoneDetail>()
                .HasOne(s => s.StoneQuality)
                .WithMany(sq => sq.StoneDetails)
                .HasForeignKey(s => s.StoneQualityId)
                .OnDelete(DeleteBehavior.Restrict);

            // CartItem relationships
            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.User)
                .WithMany(u => u.CartItems)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.Item)
                .WithMany(i => i.CartItems)
                .HasForeignKey(c => c.StyleCode)
                .OnDelete(DeleteBehavior.Restrict);

            // Order relationships
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderItem relationships
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Item)
                .WithMany(i => i.OrderItems)
                .HasForeignKey(oi => oi.StyleCode)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderStatusHistory relationships
            modelBuilder.Entity<OrderStatusHistory>()
                .HasOne(osh => osh.Order)
                .WithMany(o => o.StatusHistories)
                .HasForeignKey(osh => osh.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Inquiry relationships
            modelBuilder.Entity<Inquiry>()
                .HasOne(i => i.User)
                .WithMany(u => u.Inquiries)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Inquiry>()
                .HasOne(i => i.Item)
                .WithMany()
                .HasForeignKey(i => i.StyleCode)
                .OnDelete(DeleteBehavior.SetNull);

            // Wishlist relationships
            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.Item)
                .WithMany()
                .HasForeignKey(w => w.StyleCode)
                .OnDelete(DeleteBehavior.Restrict);

            // Banner relationships
            modelBuilder.Entity<Banner>()
                .HasOne(b => b.DiscountScheme)
                .WithMany()
                .HasForeignKey(b => b.DiscountSchemeId)
                .OnDelete(DeleteBehavior.SetNull);
        }

        private void ConfigureIndexes(ModelBuilder modelBuilder)
        {
            // Item indexes for search performance
            modelBuilder.Entity<Item>()
                .HasIndex(i => i.BrandId);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.CategoryId);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.ProductTypeId);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.GoldTypeId);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.MRP);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.IsActive);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.IsFeatured);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.IsNewLaunch);

            // Composite index for common search queries
            modelBuilder.Entity<Item>()
                .HasIndex(i => new { i.CategoryId, i.BrandId, i.IsActive });

            // CartItem indexes
            modelBuilder.Entity<CartItem>()
                .HasIndex(c => c.UserId);

            modelBuilder.Entity<CartItem>()
                .HasIndex(c => new { c.UserId, c.StyleCode })
                .IsUnique();

            // Order indexes
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.UserId);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderDate);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderStatus);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.PaymentStatus);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.IsActive);

            // Order Status Enum Conversion
            modelBuilder.Entity<Order>()
                .Property(o => o.OrderStatus)
                .HasConversion<string>();

            modelBuilder.Entity<OrderStatusHistory>()
                .Property(osh => osh.Status)
                .HasConversion<string>();

            // Wishlist indexes
            modelBuilder.Entity<Wishlist>()
                .HasIndex(w => new { w.UserId, w.StyleCode })
                .IsUnique();

            // Newsletter subscription index
            modelBuilder.Entity<NewsletterSubscription>()
                .HasIndex(n => n.Email)
                .IsUnique();

            // Notification indexes
            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.IsRead);

            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.CreatedAt);

            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.TargetRole, n.IsRead });

            // Configure Concurrency Tokens (RowVersion) for Master Entities
            ConfigureConcurrencyTokens(modelBuilder);
        }

        private void ConfigureConcurrencyTokens(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Brand>().Property(b => b.RowVersion).IsRowVersion();
            modelBuilder.Entity<Category>().Property(c => c.RowVersion).IsRowVersion();
            modelBuilder.Entity<Certificate>().Property(c => c.RowVersion).IsRowVersion();
            modelBuilder.Entity<ProductType>().Property(p => p.RowVersion).IsRowVersion();
            modelBuilder.Entity<DiamondQuality>().Property(d => d.RowVersion).IsRowVersion();
            modelBuilder.Entity<StoneQuality>().Property(s => s.RowVersion).IsRowVersion();
            modelBuilder.Entity<GoldKarat>().Property(g => g.RowVersion).IsRowVersion();
        }

        private void ConfigureDecimalPrecision(ModelBuilder modelBuilder)
        {
            // This is handled by Column attributes in the models
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Seed Certificates
            modelBuilder.Entity<Certificate>().HasData(
                new Certificate { CertificateId = 1, CertifyType = "BIS 916", IssuingAuthority = "Bureau of Indian Standards (HM)", IsActive = true, CreatedDate = seedDate },
                new Certificate { CertificateId = 2, CertifyType = "IGI", IssuingAuthority = "International Gemological Institute", IsActive = true, CreatedDate = seedDate },
                new Certificate { CertificateId = 3, CertifyType = "GIA", IssuingAuthority = "Gemological Institute of America", IsActive = true, CreatedDate = seedDate }
            );

            // Seed Gold Karats
            modelBuilder.Entity<GoldKarat>().HasData(
                new GoldKarat { GoldTypeId = 1, GoldCarat = "24K", PurityPercentage = 99.9m, Description = "Pure Gold (Coins/Bars)", IsActive = true, CreatedDate = seedDate },
                new GoldKarat { GoldTypeId = 2, GoldCarat = "22K", PurityPercentage = 91.6m, Description = "Standard for Gold Jewelry", IsActive = true, CreatedDate = seedDate },
                new GoldKarat { GoldTypeId = 3, GoldCarat = "18K", PurityPercentage = 75.0m, Description = "Standard for Diamond Jewelry", IsActive = true, CreatedDate = seedDate }
            );

            // Seed Diamond Qualities
            modelBuilder.Entity<DiamondQuality>().HasData(
                new DiamondQuality { DiamondQualityId = 1, QualityGrade = "VVS", Clarity = "Very Very Slightly Included", Color = "D-H", Cut = "Excellent", IsActive = true, CreatedDate = seedDate },
                new DiamondQuality { DiamondQualityId = 2, QualityGrade = "VS", Clarity = "Very Slightly Included", Color = "G-J", Cut = "Very Good", IsActive = true, CreatedDate = seedDate },
                new DiamondQuality { DiamondQualityId = 3, QualityGrade = "SI", Clarity = "Slightly Included", Color = "H-K", Cut = "Good", IsActive = true, CreatedDate = seedDate },
                new DiamondQuality { DiamondQualityId = 4, QualityGrade = "AD", Clarity = "Artificial (American Diamond)", Color = "White", Cut = "Good", IsActive = true, CreatedDate = seedDate }
            );

            // Seed Stone Qualities
            modelBuilder.Entity<StoneQuality>().HasData(
                new StoneQuality { StoneQualityId = 1, StoneType = "Ruby", QualityGrade = "Standard", Color = "Red", Origin = "Burma", IsActive = true, CreatedDate = seedDate },
                new StoneQuality { StoneQualityId = 2, StoneType = "Emerald", QualityGrade = "Standard", Color = "Green", Origin = "Colombia", IsActive = true, CreatedDate = seedDate },
                new StoneQuality { StoneQualityId = 3, StoneType = "Sapphire", QualityGrade = "Standard", Color = "Blue", Origin = "Kashmir", IsActive = true, CreatedDate = seedDate },
                new StoneQuality { StoneQualityId = 4, StoneType = "Pearl", QualityGrade = "AAA", Color = "White", Origin = "South Sea", IsActive = true, CreatedDate = seedDate },
                new StoneQuality { StoneQualityId = 5, StoneType = "Topaz", QualityGrade = "A", Color = "Blue", Origin = "Brazil", IsActive = true, CreatedDate = seedDate },
                new StoneQuality { StoneQualityId = 6, StoneType = "Amethyst", QualityGrade = "A", Color = "Purple", Origin = "Brazil", IsActive = true, CreatedDate = seedDate },
                new StoneQuality { StoneQualityId = 7, StoneType = "Meena", QualityGrade = "Standard", Color = "Multicolor", Origin = "India", IsActive = true, CreatedDate = seedDate }
            );
        }
    }
}
