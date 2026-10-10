using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Yash_Gems___Jewelleries.Data;
using Yash_Gems___Jewelleries.Helpers;
using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.Models.Metadata;
using Yash_Gems___Jewelleries.Repositories;
using Yash_Gems___Jewelleries.Services;
using Yash_Gems___Jewelleries.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authentication.Google;
using AspNet.Security.OAuth.GitHub;
using AspNetCoreRateLimit;
using Serilog;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// Load rate limiting configuration
builder.Configuration.AddJsonFile("appsettings.RateLimiting.json", optional: false, reloadOnChange: true);

// Set QuestPDF License
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("AppConn")
    ?? Environment.GetEnvironmentVariable("YASH_DB_CONNECTION")
    ?? throw new InvalidOperationException("Connection string not found. Set 'AppConn' in appsettings.json or 'YASH_DB_CONNECTION' environment variable.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = !builder.Environment.IsDevelopment();
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services
    .AddAuthentication()
    .AddGoogle("Google", options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]
            ?? Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")
            ?? throw new InvalidOperationException("Google ClientId not configured. Set 'Authentication:Google:ClientId' in appsettings.json or 'GOOGLE_CLIENT_ID' environment variable.");
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]
            ?? Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET")
            ?? throw new InvalidOperationException("Google ClientSecret not configured. Set 'Authentication:Google:ClientSecret' in appsettings.json or 'GOOGLE_CLIENT_SECRET' environment variable.");
    })
    .AddGitHub("GitHub", options =>
    {
        options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"]
            ?? Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID")
            ?? throw new InvalidOperationException("GitHub ClientId not configured. Set 'Authentication:GitHub:ClientId' in appsettings.json or 'GITHUB_CLIENT_ID' environment variable.");
        options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"]
            ?? Environment.GetEnvironmentVariable("GITHUB_CLIENT_SECRET")
            ?? throw new InvalidOperationException("GitHub ClientSecret not configured. Set 'Authentication:GitHub:ClientSecret' in appsettings.json or 'GITHUB_CLIENT_SECRET' environment variable.");
        options.Scope.Add("user:email");
    });

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// Configure session for shopping cart
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add HTTP Context Accessor
builder.Services.AddHttpContextAccessor();

builder.Services.AddResponseCaching();

// Configure Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "image/svg+xml", "application/atom+xml" });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

// Configure Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<AspNetCoreRateLimit.IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<AspNetCoreRateLimit.IRateLimitConfiguration, AspNetCoreRateLimit.RateLimitConfiguration>();

// Register Repositories
builder.Services.AddScoped(typeof(IMasterDataRepository<>), typeof(MasterDataRepository<>));
builder.Services.AddScoped<IBannerRepository, BannerRepository>();

// Register Core Services
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IMasterLookupService, MasterLookupService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<ILayoutService, LayoutService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IOrderEmailService, OrderEmailService>();
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSender>();
builder.Services.AddScoped<INewsletterService, NewsletterService>();
builder.Services.AddScoped<IDiscountService, DiscountService>();
builder.Services.AddScoped<IBannerService, BannerService>();

// Register Entity-Specific Master Services (for backward compatibility if needed)
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<IProductTypeService, ProductTypeService>();
builder.Services.AddScoped<IDiamondQualityService, DiamondQualityService>();
builder.Services.AddScoped<IStoneQualityService, StoneQualityService>();
builder.Services.AddScoped<IGoldKaratService, GoldKaratService>();
builder.Services.AddScoped<IInquiryService, InquiryService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Register Master Data Metadata Provider
builder.Services.AddSingleton<IEntityMetadataProvider, EntityMetadataProvider>();

var app = builder.Build();

// Seed roles and admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Seed sample data for testing
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbInitializer.SeedAllSampleDataAsync(services);

        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Sample data seeded successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding sample data.");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Enable Response Compression
if (!app.Environment.IsDevelopment())
{
    app.UseResponseCompression();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
    
// Enable Response Caching
app.UseResponseCaching();

// Enable IP-based rate limiting
app.UseIpRateLimiting();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Restrict admin users to admin panel
app.UseMiddleware<Yash_Gems___Jewelleries.Middleware.AdminAccessRestrictionMiddleware>();

app.UseSession();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.MapHealthChecks("/health");
app.MapHub<Yash_Gems___Jewelleries.Hubs.NotificationHub>("/notificationHub");

app.Run();

// Seed roles and admin user
static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Create roles
    string[] roleNames = { "Admin", "Customer" };

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Create default admin user
    var adminEmail = "admin@yash.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var newAdmin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Yash",
            LastName = "Admin",
            EmailConfirmed = true,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            PhoneNumber = "+92-9876543210",
            Address = "Yash Gems & Jewelleries Store, Main Branch",
            City = "Karachi",
            State = "Pakistan",
            PostalCode = "74000"
        };

        var result = await userManager.CreateAsync(newAdmin, "Admin@12345");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(newAdmin, "Admin");
        }
    }
}
