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

                // Extraer el ID del pago
                string MerchantOrdersID = _mercadoPagoServices.ExtractPaymentId(resource);

                _logger.LogInformation("Extracción del ID del pago: {OrderId}", MerchantOrdersID);

                string accessToken = await _generalServices.ObtenerData("uspACCESS_TOKENCsv", "");

                // Verificar el token de acceso
                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogError("El AccessToken es nulo o vacío.");
                    return StatusCode(500, "Token de acceso no válido");
                }

                _logger.LogInformation("Access Token obtenido: {AccessToken}", accessToken);

                var merchant_ordersJsonResponse = await _generalServices.GetAsync(
                    MERCADOPAGO_API_URL_merchant_orders,
                    MerchantOrdersID,
                    bearerToken: accessToken);

                _logger.LogInformation("Respuesta de la API de 'merchant_orders': {Response}", merchant_ordersJsonResponse);

                JsonDocument jsonDoc = null;
                try
                {
                    jsonDoc = JsonDocument.Parse(merchant_ordersJsonResponse);
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Error al parsear la respuesta JSON de 'merchant_orders'");
                    return StatusCode(500, "Error al procesar la respuesta de 'merchant_orders'");
                }

                var rootOrder = jsonDoc.RootElement;

                // Orden
                long ordenId = rootOrder.GetProperty("id").GetInt64();
                string externalReference = rootOrder.GetProperty("external_reference").GetString();
                string orderStatus = rootOrder.GetProperty("status").GetString();
                string orderPaidStatus = rootOrder.GetProperty("order_status").GetString();
                decimal totalAmount = rootOrder.GetProperty("total_amount").GetDecimal();
                decimal paidAmount = rootOrder.GetProperty("paid_amount").GetDecimal();
                decimal refundedAmount = rootOrder.GetProperty("refunded_amount").GetDecimal();
                string dateCreated = rootOrder.GetProperty("date_created").GetString();
                string lastUpdated = rootOrder.GetProperty("last_updated").GetString();
                string notificationUrl = rootOrder.GetProperty("notification_url").GetString();

                // Pagos asociados
                var payment = rootOrder.GetProperty("payments")[0];
                long paymentId = payment.GetProperty("id").GetInt64();
                decimal transactionAmount = payment.GetProperty("transaction_amount").GetDecimal();
                string paymentStatus = payment.GetProperty("status").GetString();
                string currencyId = payment.GetProperty("currency_id").GetString();
                string dateApproved = payment.GetProperty("date_approved").GetString();

                // Pagador
                long payerId = rootOrder.GetProperty("payer").GetProperty("id").GetInt64();

                // Artículos comprados
                var items = rootOrder.GetProperty("items");
                List<string> ids = new List<string>();

                foreach (var item in items.EnumerateArray())
                {
                    string IdProducto = item.GetProperty("id").GetString();
                    ids.Add(IdProducto);
                }
                string concatenatedIds = string.Join(",", ids);

                // Realizar la solicitud a la API de MercadoPago
                var paymentsJsonResponse = await _generalServices.GetAsync(
                    MERCADOPAGO_API_URL_payments,
                    paymentId.ToString(),
                    bearerToken: accessToken);

                _logger.LogInformation("Respuesta de la API de 'payments': {Response}", paymentsJsonResponse);

                JsonDocument jsonDocPayments = null;
                try
                {
                    jsonDocPayments = JsonDocument.Parse(paymentsJsonResponse);
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Error al parsear la respuesta JSON de 'payments'");
                    return StatusCode(500, "Error al procesar la respuesta de 'payments'");
                }

                var rootPayments = jsonDocPayments.RootElement;
                _logger.LogInformation("Respuesta de 'payments' parseada: {RootPayments}", rootPayments);

                // Información de la transacción
                long transactionId = rootPayments.GetProperty("id").GetInt64();
                string status = rootPayments.GetProperty("status").GetString();
                string statusDetail = rootPayments.GetProperty("status_detail").GetString();
                int installments = rootPayments.GetProperty("installments").GetInt32();
                string operationType = rootPayments.GetProperty("operation_type").GetString();
                string paymentMethodId = rootPayments.GetProperty("payment_method_id").GetString();
                string paymentTypeId = rootPayments.GetProperty("payment_type_id").GetString();
                string statementDescriptor = rootPayments.GetProperty("statement_descriptor").GetString();

                // Tarjeta
                var card = rootPayments.GetProperty("card");
                var firstSixDigits = card.GetProperty("first_six_digits").GetString();
                var lastFourDigits = card.GetProperty("last_four_digits").GetString();
                var expirationMonth = 0; // card.GetProperty("expiration_month").GetInt32();
                if (card.TryGetProperty("expiration_month", out JsonElement expirationMonthElement))
                {
                    if (expirationMonthElement.ValueKind == JsonValueKind.Number)
                    {
                        expirationMonth = expirationMonthElement.GetInt32();
                    }
                }

                var expirationYear = 0; // card.GetProperty("expiration_year").GetInt32();
                if (card.TryGetProperty("expiration_year", out JsonElement expirationYearElement))
                {
                    if (expirationYearElement.ValueKind == JsonValueKind.Number)
                    {
                        expirationYear = expirationYearElement.GetInt32();
                    }
                }

                var cardholder = card.GetProperty("cardholder");
                var cardholderName = cardholder.GetProperty("name").GetString();
                var cardholderIdentification = cardholder.GetProperty("identification");
                var cardholderIdNumber = cardholderIdentification.GetProperty("number").GetString();
                var cardholderIdType = cardholderIdentification.GetProperty("type").GetString();

                // Pagador
                var payer = rootPayments.GetProperty("payer");
                var payerEmail = payer.GetProperty("email").GetString();

                string payerStreetName = null;
                string payerStreetNumber = null;
                string payerZipCode = null;

                if (payer.TryGetProperty("address", out JsonElement payerAddress))
                {
                    payerStreetName = payerAddress.TryGetProperty("street_name", out JsonElement streetName) ? streetName.GetString() : null;
                    payerStreetNumber = payerAddress.TryGetProperty("street_number", out JsonElement streetNumber) ? streetNumber.GetString() : null;
                    payerZipCode = payerAddress.TryGetProperty("zip_code", out JsonElement zipCode) ? zipCode.GetString() : null;
                }
                var payerPhone = payer.GetProperty("phone");
                var payerPhoneNumber = payerPhone.GetProperty("number").GetString();
                var payerFirstName = payer.GetProperty("first_name").GetString();
                var payerLastName = payer.GetProperty("last_name").GetString();

                // Detalles de comisión
                var feeAmount = 0.0;
                var feeType = "";

                if (rootPayments.TryGetProperty("fee_details", out JsonElement feeDetailsElement))
                {
                    if (feeDetailsElement.ValueKind == JsonValueKind.Array && feeDetailsElement.GetArrayLength() > 0)
                    {
                        var feeDetails = feeDetailsElement[0];
                        feeAmount = (double) feeDetails.GetProperty("amount").GetDecimal();
                        feeType = feeDetails.GetProperty("type").GetString();
                    }
                }

                var data = "" + ordenId + '¯' + externalReference + '¯' + orderStatus + '¯' +
                    orderPaidStatus + '¯' + totalAmount + '¯' + paidAmount + '¯' + refundedAmount + '¯' +
                    currencyId + '¯' + dateCreated + '¯' + lastUpdated + '¯' + notificationUrl + '¯' +
                    paymentId + '¯' + transactionAmount + '¯' + paymentStatus + '¯' + dateApproved + '¯' +
                    payerId + '¯' + concatenatedIds + '¯' + status + '¯' +
                    statusDetail + '¯' + installments + '¯' + operationType + '¯' + paymentMethodId + '¯' +
                    paymentTypeId + '¯' + statementDescriptor + '¯' + firstSixDigits + '¯' + lastFourDigits + '¯' +
                    expirationMonth + '¯' + expirationYear + '¯' + cardholderName + '¯' + cardholderIdNumber + '¯' +
                    cardholderIdType + '¯' + payerEmail + '¯' + payerStreetName + '¯' + payerStreetNumber + '¯' +
                    payerZipCode + '¯' + payerPhoneNumber + '¯' + payerFirstName + '¯' + payerLastName + '¯' +
                    feeAmount + '¯' + feeType;

                _logger.LogInformation("Datos procesados: {Data}", data);

                await _generalServices.ObtenerData("uspInsertarTransaccionesCsv", data);

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
