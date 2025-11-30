using Microsoft.AspNetCore.Builder;

namespace BaseSystem.Middleware
{
    public static class PermissionMiddlewareExtensions
    {
        public static IApplicationBuilder UsePermission(this IApplicationBuilder builder, string permission)
        {
            return builder.UseMiddleware<PermissionMiddleware>(permission);
        }
    }
}