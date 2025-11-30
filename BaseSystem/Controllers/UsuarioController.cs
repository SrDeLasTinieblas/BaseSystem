using Biblioteca.Infrastructure.Services;
using Biblioteca.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BaseSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly GeneralServices _GeneralServices;
        private readonly UsuarioServices _usuarioServices;
        private readonly RefreshTokenService _refreshTokenService;
        private readonly JWTServices _jWTServices;

        public UsuarioController(GeneralServices generalServices, UsuarioServices usuarioServices, RefreshTokenService refreshTokenService, JWTServices jWTServices)
        {
            _GeneralServices = generalServices;
            _usuarioServices = usuarioServices;
            _refreshTokenService = refreshTokenService;
            _jWTServices = jWTServices;
        }

        [Authorize(Policy = "RequireSuperAdminRole")] // PARA CONTROLAR QUIENES PUEDEN TENER ACCESO A ESTA ENDPOINT
        [HttpGet("ObtenerUsuarios")]
        public async Task<IActionResult> GetUsuario()
        {
            string? emailFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(emailFromToken))
            {
                return Unauthorized("No hay email en el token"); // No hay email en el token
            }

            // Verificar el rol real del usuario en la base de datos
            var tokenReal = await _GeneralServices.ObtenerData("uspObtenerRoleFromTokenCsv", emailFromToken);
            if (tokenReal == null)
            {
                return Unauthorized("Usuario no encontrado"); // Usuario no encontrado 401
            }

            //if (tokenReal != "SuperAdmin" || tokenReal != "Administrador")
            //{
            //    return Forbid("Acceso denegado"); // Acceso denegado 403
            //}

            var response = await _GeneralServices.ObtenerData("uspListarUsuarioCsv", "");
            if (response == null)
                return NotFound("No hay datos del procedimiento uspListarUsuarioCsv"); // 404

            return Ok(response); // 200
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Aquí deberías obtener los permisos reales del usuario desde la BD
            var permisos = new List<string> { "admin:read" }; // Simulación
            var token = await _usuarioServices.AuthenticateUsuario($"{request.Email}|{request.Password}", permisos);
            if (token.StartsWith("E|"))
                return Unauthorized(token);
            var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(request.Email);
            return Ok(new { token, refreshToken = refreshToken.Token });
        }

        [AllowAnonymous]
        [HttpPost("login-social")]
        public async Task<IActionResult> LoginSocial([FromBody] SocialLoginRequest request)
        {
            // Aquí deberías validar el token con Google/Facebook usando sus SDKs o endpoints
            // Por simplicidad, solo se simula la validación
            var isValid = await ValidateSocialToken(request.Provider, request.Token);
            if (!isValid)
                return Unauthorized("Token social inválido");
            // Buscar usuario por email, si no existe, registrar
            var email = request.Email; // En real, extraer del token social
            // Aquí podrías obtener el rol desde la BD o asignar uno por defecto
            var token = _jWTServices.GenerateJwtToken(email, "User", 60);
            var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(email);
            return Ok(new { token, refreshToken = refreshToken.Token });
        }

        private async Task<bool> ValidateSocialToken(string provider, string token)
        {
            // Aquí deberías llamar a Google/Facebook para validar el token
            // Retornar true si es válido, false si no
            await Task.Delay(100); // Simulación
            return true;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var data = $"{request.Email}|{request.Nombre}|{request.Password}";
            var result = await _usuarioServices.RegisterUsuario(data);
            if (result.StartsWith("E|"))
                return BadRequest(result);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var refreshToken = await _refreshTokenService.GetRefreshTokenAsync(request.RefreshToken);
            if (refreshToken == null || refreshToken.ExpiryDate < DateTime.UtcNow || refreshToken.IsRevoked)
                return Unauthorized("Refresh token inválido o expirado");
            // Aquí podrías obtener el email del usuario desde el refresh token
            var token = await _usuarioServices.AuthenticateUsuario($"{refreshToken.UserEmail}|REFRESH_TOKEN");
            return Ok(new { token });
        }

        [Authorize]
        [HttpGet("protegido-admin")]
        public IActionResult ProtegidoAdmin()
        {
            // Este endpoint requiere el permiso "admin:read"
            // El middleware puede ser aplicado globalmente o aquí manualmente
            var tienePermiso = User.Claims.Any(c => c.Type == "permission" && c.Value == "admin:read");
            if (!tienePermiso)
                return Forbid("No tienes permisos de admin:read");
            return Ok("Acceso permitido a admin:read");
        }

#nullable enable
        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
        public class RegisterRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
        public class RefreshRequest
        {
            public string RefreshToken { get; set; } = string.Empty;
        }
        public class SocialLoginRequest
        {
            public string Provider { get; set; } = string.Empty; // "google" o "facebook"
            public string Token { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty; // En real, extraer del token
        }
#nullable disable
    }
}
