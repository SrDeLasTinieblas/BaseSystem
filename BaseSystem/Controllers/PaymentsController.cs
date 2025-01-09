using Biblioteca.Domain.Entities;
using Biblioteca.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BaseSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly MercadoPagoServices _mercadoPagoServices;
        private readonly GeneralServices _generalServices;

        public PaymentsController(GeneralServices generalServices, MercadoPagoServices mercadoPagoServices)
        {
            _mercadoPagoServices = mercadoPagoServices;
            _generalServices = generalServices;
        }

        [HttpGet("CreateCheckoutPreference")]
        public async Task<IActionResult> CrearPago(string data)
        {
            string jsonResponse = await _mercadoPagoServices.CreateCheckoutPreferenceAsync(data);
            return Ok(jsonResponse);
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> PaymentWebhook([FromBody] MercadoPagoNotification notification)
        {
            try
            {
                // Obtener el ID de la preferencia desde la notificación
                var preferenceId = notification.PreferenceId;

                // Verificar el estado del pago
                var paymentStatus = notification.Status;

                // Procesar la notificación según el estado del pago
                if (paymentStatus == "approved")
                {
                    // Ejecutar el procedimiento para procesar el pago exitoso
                    await _generalServices.ObtenerData("", preferenceId);
                }
                else if (paymentStatus == "pending")
                {
                    // Si el pago está pendiente, puedes tomar alguna acción
                    await _generalServices.ObtenerData("", preferenceId);
                }
                else if (paymentStatus == "rejected")
                {
                    // Si el pago fue rechazado, puedes manejarlo también
                    await _generalServices.ObtenerData("", preferenceId);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return BadRequest(ex.Message);
            }
        }



    }
}
