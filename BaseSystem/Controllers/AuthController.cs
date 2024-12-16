using Biblioteca.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace BaseSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly GeneralServices _GeneralServices;
        private readonly UsuarioServices _usuarioServices;

        public AuthController(GeneralServices generalServices, UsuarioServices usuarioServices)
        {
            _GeneralServices = generalServices;
            _usuarioServices = usuarioServices;
        }

        [HttpGet("Login")]
        public async Task<IActionResult> Login(string data) // StevenLee89@live.com|PassTextPlain
        {
            var response = await _usuarioServices.AuthenticateUsuario(data);
            if (response == null)
                return NotFound();

            return Ok(response);
        }

        [HttpPost("RegisterUsuario")]
        public async Task<IActionResult> RegisterAlumno(string data) // 1|JhonTheRipper@gmail.com|PassTextPlain|Jhon|TheRipper|https://fotoperfil.com
        {
            var response = await _usuarioServices.RegisterUsuario(data);
            if (response == null)
                return NotFound();

            return Ok(response);
        }


    }
}
