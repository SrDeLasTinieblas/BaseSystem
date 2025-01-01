using Biblioteca.Infrastructure.Services;
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


        public UsuarioController(GeneralServices generalServices)
        {
            _GeneralServices = generalServices;
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


    }
}
