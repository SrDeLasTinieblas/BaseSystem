using Biblioteca.Domain.Entities;
using Biblioteca.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BaseSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly MercadoPagoServices _mercadoPagoServices;
        private readonly GeneralServices _generalServices;
        private readonly ILogger<PaymentsController> _logger;
        private const string MERCADOPAGO_API_URL = "https://api.mercadopago.com/v1/payments";

        public PaymentsController(GeneralServices generalServices, MercadoPagoServices mercadoPagoServices, ILogger<PaymentsController> logger)
        {
            _mercadoPagoServices = mercadoPagoServices;
            _generalServices = generalServices;
            _logger = logger;
        }

        [HttpGet("CreateCheckoutPreference")]
        public async Task<IActionResult> CrearPago(string data)
        {
            string jsonResponse = await _mercadoPagoServices.CreateCheckoutPreferenceAsync(data);
            return Ok(jsonResponse);
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> PaymentWebhook([FromBody] JsonElement payload)
        {
            try
            {
                _logger.LogInformation("Webhook recibido de MercadoPago: {Data}", payload);

                // Validar el payload
                if (!IsValidPayload(payload, out string topic, out string resource))
                {
                    _logger.LogWarning("Payload inválido recibido: {Payload}", payload);
                    return BadRequest("Payload inválido");
                }

                _logger.LogInformation("Tema del webhook: {Topic}, recurso: {Resource}", topic, resource);

                // Extraer el ID del pago
                string paymentId = ExtractPaymentId(resource);

                // Obtener el token de acceso
                string accessToken = await _generalServices.ObtenerData("uspACCESS_TOKENCsv", "");

                // Realizar la solicitud a la API de MercadoPago
                var payment = await _generalServices.GetAsync<MercadoPagoNotification>(
                    MERCADOPAGO_API_URL,
                    paymentId,
                    bearerToken: accessToken);

                // Procesar el pago
                await _generalServices.ObtenerData("uspTestCsv", JsonSerializer.Serialize(payment));

                return Ok(new { message = "Webhook procesado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar el webhook");
                return StatusCode(500, "Error interno al procesar el webhook");
            }
        }

        private bool IsValidPayload(JsonElement payload, out string topic, out string resource)
        {
            topic = null;
            resource = null;

            if (!payload.TryGetProperty("topic", out JsonElement topicElement) ||
                !payload.TryGetProperty("resource", out JsonElement resourceElement))
            {
                return false;
            }

            topic = topicElement.GetString();
            resource = resourceElement.GetString();
            return !string.IsNullOrEmpty(topic) && !string.IsNullOrEmpty(resource);
        }

        private string ExtractPaymentId(string resource)
        {
            return resource.Split('/').Last();
        }





    }
}
