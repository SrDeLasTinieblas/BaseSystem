using Biblioteca.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BaseSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : Controller
    {
        private readonly GeneralServices _GeneralServices;


        public UsuarioController(GeneralServices generalServices)
        {
            _GeneralServices = generalServices;
        }

        [Authorize(Roles = "SuperAdmin")] // PARA CONTROLAR QUIEN PUEDE EJECUTAR ESTE ENDPOINT.
        [HttpGet("ObtenerUsuarios")]
        public async Task<IActionResult> GetUsuario()
        {
            var response = await _GeneralServices.ObtenerData("uspListarUsuarioCsv", "");
            string? emailFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(emailFromToken))
            {
                return Unauthorized("No hay email en el token"); // No hay email en el token
            }

            // Verificar el rol real del usuario en la base de datos
            var tokenReal = await _GeneralServices.ObtenerData("uspObtenerRoleFromTokenCsv", emailFromToken);

            if (tokenReal == null)
            {
                return Unauthorized(); // Usuario no encontrado
            }

            if (tokenReal != "SuperAdmin" || tokenReal != "Administrador")
            {
                return Forbid(); // Acceso denegado
            }

            if (response == null)
                return NotFound();

            return Ok(response);
        }


    }
}
