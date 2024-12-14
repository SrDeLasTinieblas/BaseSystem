using Biblioteca.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace BaseSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly GeneralServices _GeneralServices;
        public AuthController(GeneralServices generalServices)
        {
            _GeneralServices = generalServices;
        }

        //[Authorize]
        [HttpGet("Login")]
        public async Task<IActionResult> GetUsuario(string data)
        {
            var response = await _GeneralServices.ObtenerData("uspLoginCsv", data);

            if (response == null)
                return NotFound();

            return Ok(response);
        }

    }
}
