using Biblioteca.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace BaseSystem.Controllers
{
    public class EmailController : Controller
    {
        private readonly GeneralServices _generalServices;
        private readonly EmailServices _emailServices;

        public EmailController(GeneralServices generalServices, EmailServices emailServices)
        {
            _generalServices = generalServices;
            _emailServices = emailServices;
        }

        [HttpGet("sendEmail")]
        public async Task<IActionResult> SendEmail(string email, string Subject)
        {
            try
            {
                var codigoGenerado = _emailServices.GenerateVerificationCode();
                await _emailServices.SendVerificationEmail(email, Subject, codigoGenerado);
                return Ok(codigoGenerado);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al enviar el email: {ex.Message}");
            }
        }

    }
}
