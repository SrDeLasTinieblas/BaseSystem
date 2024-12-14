using Biblioteca.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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


        //[Authorize]
        [HttpGet("ObtenerUsuarios")]
        public async Task<IActionResult> GetUsuario()
        {
            var response = await _GeneralServices.ObtenerData("uspListarUsuarioCsv", "");

            if (response == null)
                return NotFound();

            return Ok(response);
        }


    }
}
