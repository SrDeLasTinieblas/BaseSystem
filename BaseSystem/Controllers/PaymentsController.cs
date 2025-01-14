using Biblioteca.Domain.Entities;
using Biblioteca.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Globalization;
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
        //private const string MERCADOPAGO_API_URL = "https://api.mercadopago.com/v1/payments";
        private const string MERCADOPAGO_API_URL = "https://api.mercadolibre.com/merchant_orders";

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
                string orderId = ExtractPaymentId(resource);

                string accessToken = await _generalServices.ObtenerData("uspACCESS_TOKENCsv", "");

                // Realizar la solicitud a la API de MercadoPago
                var jsonResponse = await _generalServices.GetAsync(
                    MERCADOPAGO_API_URL,
                    orderId,
                    bearerToken: accessToken);

                var jsonDoc = JsonDocument.Parse(jsonResponse);
                var root = jsonDoc.RootElement;

                var transactionId = root.GetProperty("id").GetInt64();
                var externalReference = root.GetProperty("external_reference").GetString();
                var status = root.GetProperty("status").GetString();
                var statusDetail = root.GetProperty("payments").EnumerateArray().FirstOrDefault().GetProperty("status_detail").GetString();
                var totalAmount = root.GetProperty("total_amount").GetDecimal();
                var currencyId = root.GetProperty("payments").EnumerateArray().FirstOrDefault().GetProperty("currency_id").GetString();
                var userId = root.GetProperty("collector").GetProperty("id").GetInt64();
                var paymentId = root.GetProperty("payments").EnumerateArray().FirstOrDefault().GetProperty("id").GetInt64();
                var preferenceId = root.GetProperty("preference_id").GetString();
                var dateApproved = root.GetProperty("payments").EnumerateArray().FirstOrDefault().GetProperty("date_approved").GetDateTime();
                var dateCreatedStr = root.GetProperty("date_created").GetString();
                var dateUpdatedStr = root.GetProperty("last_updated").GetString();

                var dateCreated = DateTimeOffset.Parse(dateCreatedStr, null, System.Globalization.DateTimeStyles.AssumeUniversal).DateTime;
                var dateUpdated = DateTimeOffset.Parse(dateUpdatedStr, null, System.Globalization.DateTimeStyles.AssumeUniversal).DateTime;

                var data = "" + transactionId + '¯' + externalReference + '¯' + status + '¯' + statusDetail + '¯' + 
                    totalAmount + '¯' + currencyId + '¯' + userId + '¯' + paymentId + '¯' + preferenceId + '¯' + 
                    dateApproved + '¯' +  dateCreated + '¯' + dateUpdated+ '¯' + root;

                _logger.LogInformation("{data}", data);

                await _generalServices.ObtenerData("uspInsertarTransaccionesCsv", data);

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
