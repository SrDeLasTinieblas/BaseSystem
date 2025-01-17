using Biblioteca.Domain.Entities;
using Biblioteca.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Text.Json;
using JsonException = Newtonsoft.Json.JsonException;
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
        private const string MERCADOPAGO_API_URL_payments = "https://api.mercadopago.com/v1/payments";
        private const string MERCADOPAGO_API_URL_merchant_orders = "https://api.mercadolibre.com/merchant_orders";

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
                if (!_mercadoPagoServices.IsValidPayload(payload, out string topic, out string resource))
                {
                    _logger.LogWarning("Payload inválido recibido: {Payload}", payload);
                    return BadRequest("Payload inválido");
                }

                _logger.LogInformation("Tema del webhook: {Topic}, recurso: {Resource}", topic, resource);

                // Procesar la transacción
                var result = await _mercadoPagoServices.ProcessTransaction(resource);

                if (!result.Success)
                {
                    _logger.LogError("Error al procesar la transacción: {Error}", result.ErrorMessage);
                    return StatusCode(500, result.ErrorMessage);
                }

                return Ok(new { message = "Webhook procesado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar el webhook: {Message}", ex.Message);
                return StatusCode(500, "Error interno al procesar el webhook");
            }
        }



    }
}
