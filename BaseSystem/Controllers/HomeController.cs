using Biblioteca.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BaseSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly GeneralServices _GeneralServices;


        public HomeController(GeneralServices generalServices)
        {
            _GeneralServices = generalServices;
        }

        [Authorize(Roles = "SuperAdmin")] // PARA CONTROLAR QUIEN PUEDE EJECUTAR ESTE ENDPOINT.
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
                return Unauthorized(); // Usuario no encontrado 401
            }

            //if (tokenReal != "SuperAdmin" || tokenReal != "Administrador")
            //{
            //    return Forbid(); // Acceso denegado 403
            //}

            var response = await _GeneralServices.ObtenerData("uspListarUsuarioCsv", "");
            if (response == null)
                return NotFound(); // 404

            return Ok(response); // 200
        }


    }
}
