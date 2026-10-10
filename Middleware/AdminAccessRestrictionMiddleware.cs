using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Linq;

namespace Yash_Gems___Jewelleries.Middleware
{
    public class AdminAccessRestrictionMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly string[] WhitelistedPaths = 
        { 
            "/Account/Logout", 
            "/Account/AccessDenied", 
            "/notificationHub", 
            "/health",
            "/Home/Error"
        };

        public AdminAccessRestrictionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var user = context.User;
            var path = context.Request.Path.Value ?? string.Empty;

            if (user.Identity != null && user.Identity.IsAuthenticated && user.IsInRole("Admin"))
            {
                // Check if the path is NOT an admin path and NOT whitelisted
                bool isAdminArea = path.StartsWith("/Admin", System.StringComparison.OrdinalIgnoreCase);
                bool isWhitelisted = WhitelistedPaths.Any(p => path.Equals(p, System.StringComparison.OrdinalIgnoreCase));

                if (!isAdminArea && !isWhitelisted)
                {
                    // Redirect to Admin Dashboard
                    context.Response.Redirect("/Admin/Dashboard/Index");
                    return;
                }
            }

            await _next(context);
        }
    }
}
