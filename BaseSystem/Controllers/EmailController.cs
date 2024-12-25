using Biblioteca.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace BaseSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly GeneralServices _generalServices;
        private readonly EmailServices _emailServices;

        public EmailController(GeneralServices generalServices, EmailServices emailServices)
        {
            _generalServices = generalServices;
            _emailServices = emailServices;
        }

        [HttpGet("sendEmail")]
        public async Task<IActionResult> SendEmail(string email, string asunto)
        {
            try
            {
                var codigoGenerado = _emailServices.GenerateVerificationCode();
                var emailServices = new EmailServices(_generalServices);

                await emailServices.SendVerificationEmail(email, asunto, codigoGenerado);
                return Ok(codigoGenerado);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al enviar el email: {ex.Message}");
            }
        }

    }
}
