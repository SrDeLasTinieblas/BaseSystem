using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BaseSystem.Middleware
{
    public class PermissionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _requiredPermission;

        public PermissionMiddleware(RequestDelegate next, string requiredPermission)
        {
            _next = next;
            _requiredPermission = requiredPermission;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var claimsIdentity = context.User.Identity as ClaimsIdentity;
                var permissions = claimsIdentity?.Claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();
                if (permissions != null && permissions.Contains(_requiredPermission))
                {
                    await _next(context);
                    return;
                }
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("No tienes permisos suficientes");
                return;
            }
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("No autenticado");
        }
    }
}