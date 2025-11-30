using Biblioteca.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Biblioteca.Infrastructure.Services
{
    public class MercadoPagoServices
    {
        private readonly GeneralServices _GeneralServices;
        private readonly ILogger<MercadoPagoServices> _logger;

        private const string MERCADOPAGO_API_URL_payments = "https://api.mercadopago.com/v1/payments/";
        private const string MERCADOPAGO_API_URL_merchant_orders = "https://api.mercadolibre.com/merchant_orders/";


        public MercadoPagoServices(GeneralServices generalServices, ILogger<MercadoPagoServices> logger)
        {
            _GeneralServices = generalServices;
            _logger = logger;
        }

        public async Task<string> CreateCheckoutPreferenceAsync(string data) // idUsuario|idProducto
        {
            // Validar que se recibieron los datos necesarios
            if (string.IsNullOrEmpty(data))
                return "No se proporcionaron los datos necesarios.";

            try
            {
                string[] datos = data.Split('|');
                if (datos.Length < 2)
                    return "Formato de datos inválido. Asegúrate de incluir: idUsuario|idProducto.";

                var DataCheckout = await _GeneralServices.ObtenerData("uspObtenerDataCheckoutCSV", data); // DataCheckout = approved¬https://hnsebciei.netlify.app¬https://hnsebciei.netlify.app¬https://hnsebciei.netlify.app¬Investigacion Asesoria¬false¬IWD1238971¬12¬account_money¬https://sergiobernales.somee.com/api/Payments/webhook¬true¬2024-01-01T12:00:00.000-04:00¬2025-12-31T12:00:00.000-04:00¯1|Formulario de Solicitud Institucional|1|100.00|Formulario para gestionar solicitudes de revisión de proyectos y enmiendas|Formularios¯1|test_user_12398378192@testuser.com|Juan|Lopez|51|980855886|DNI|74286594|Street|123|1406

                var DataConfiguracion = DataCheckout.Split("¯")[0];
                var DataProductos = DataCheckout.Split("¯")[1];
                var DataUsuarios = DataCheckout.Split("¯")[2];

                var Configuracion = DataConfiguracion.Split('¬');
                // Configuracion
                string autoReturn = Configuracion[0];
                string back_urls_success = Configuracion[1];
                string back_urls_failure = Configuracion[2];
                string back_urls_pending = Configuracion[3];
                string statementDescriptor = Configuracion[4];
                bool binaryMode = Convert.ToBoolean(Configuracion[5]);
                string externalReference = Configuracion[6];

                var Productos = DataProductos.Split('¬');
                var itemsList = new List<object>();

                // Iterar sobre cada producto
                foreach (var producto in Productos)
                {
                    var columns = producto.Split('|');

                    if (columns.Length >= 6)
                    {
                        string ID = columns[0];
                        string title = columns[1];
                        int quantity = int.Parse(columns[2]);
                        decimal unitPrice = decimal.Parse(columns[3]);
                        string Descripcion = columns[4];
                        string categoryId = columns[5];

                        // Agregar el producto formateado a la lista
                        itemsList.Add(new
                        {
                            id = ID,
                            title = title,
                            quantity = quantity,
                            unit_price = unitPrice,
                            description = Descripcion,
                            category_id = categoryId
                        });
                    }
                }
                var items = itemsList.ToArray();

                var Usuarios = DataUsuarios.Split('|');
                // Usuario
                string payerEmail = Usuarios[0]; // required
                string playerName = Usuarios[1]; // required
                string playerSurname = Usuarios[2]; // required
                string player_area_code = Usuarios[3]; // Datos no obligatorios
                string playerPhone = Usuarios[4]; // required
                string identificationType = Usuarios[5]; // Datos no obligatorios
                string identificationNumber = Usuarios[6]; // Datos no obligatorios
                string playerPhoneStreet_name = Usuarios[7]; // default
                string playerPhoneStreet_number = Usuarios[8]; // default
                string playerPhoneZip_code = Usuarios[9]; // default

                // Configuracion
                int payment_methodsInstallments = int.Parse(Configuracion[7]);
                string? payment_methodsDefault_payment_method_id = Configuracion[8] == "null" ? null : Configuracion[8];
                string notificationUrl = Configuracion[9];
                bool expires = Convert.ToBoolean(Configuracion[10]);
                string expiration_date_from = Configuracion[11];
                string expiration_date_to = Configuracion[12];

                // Configuración de métodos de pago excluidos
                object[]? excludedPaymentTypes = Configuracion[13] == "null"
                    ? null
                    : Configuracion[13].Split(',').Select(x => new { id = x.Trim() }).ToArray<object>();

                object[]? excludedPaymentMethods = Configuracion[14] == "null"
                    ? null
                    : Configuracion[14].Split(',').Select(x => new { id = x.Trim() }).ToArray<object>();


                // Crear la carga útil (payload) para el request
                var payload = new
                {
                    auto_return = autoReturn, //"approved",
                    back_urls = new
                    {
                        success = back_urls_success, //"http://httpbin.org/get?back_url=success",
                        failure = back_urls_failure, //"http://httpbin.org/get?back_url=failure",
                        pending = back_urls_pending, //"http://httpbin.org/get?back_url=pending"
                    },
                    statement_descriptor = statementDescriptor, //TestStore
                    binary_mode = binaryMode, //false,
                    external_reference = externalReference, //"IWD1238971",
                    items = items,
                    payer = new
                    {
                        email = payerEmail,
                        name = playerName,
                        surname = playerSurname,
                        phone = new
                        {
                            area_code = player_area_code,//11
                            number = playerPhone //1523164589
                        },
                        identification = new
                        {
                            type = identificationType, //"DNI",
                            number = identificationNumber //"12345678"
                        },
                        address = new
                        {
                            street_name = playerPhoneStreet_name, //"Street",
                            street_number = playerPhoneStreet_number, //123,
                            zip_code = playerPhoneZip_code //"1406"
                        }
                    },
                    payment_methods = new
                    {
                        excluded_payment_types = excludedPaymentTypes,
                        excluded_payment_methods = excludedPaymentMethods,
                        installments = payment_methodsInstallments,
                        default_payment_method_id = payment_methodsDefault_payment_method_id
                    },
                    notification_url = notificationUrl, //"https://www.your-site.com/webhook",
                    expires = expires, //true,
                    expiration_date_from = expiration_date_from, //"2024-01-01T12:00:00.000-04:00",
                    expiration_date_to = expiration_date_to //"2025-12-31T12:00:00.000-04:00"
                };

                // Obtener el token de acceso
                var TU_ACCESS_TOKEN = await _GeneralServices.ObtenerData("uspACCESS_TOKENCsv", "");

                // Crear el cliente HTTP
                using (var httpClient = new HttpClient())
                {
                    // Configurar encabezados
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + TU_ACCESS_TOKEN);

                    // Serializar el payload a JSON
                    string jsonPayload = JsonConvert.SerializeObject(payload);

                    // Enviar la solicitud POST
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync("https://api.mercadopago.com/checkout/preferences", content);

                    // Leer y procesar la respuesta
                    string responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var json = JObject.Parse(responseContent);

                        var initPoint = json["init_point"]?.ToString();
                        //var externalReference = json["external_reference"]?.ToString();

                        //var result = new
                        //{
                        //    init_point = initPoint,
                        //    external_reference = externalReference
                        //};

                        return initPoint + '¯' + externalReference;
                    }
                    else
                    {
                        return $"Error: {responseContent}";
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear la preferencia de pago", ex);
            }
        }


        public async Task<(bool Success, string ErrorMessage)> ProcessTransaction(string resource)
        {
            try
            {
                string accessToken = await _GeneralServices.ObtenerData("uspACCESS_TOKENCsv", "");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return (false, "Token de acceso no válido");
                }
                string MerchantOrdersID = ExtractPaymentId(resource);

                // Obtener detalles del pedido
                var merchantOrdersString = await _GeneralServices.GetAsync(
                    MERCADOPAGO_API_URL_merchant_orders,
                    MerchantOrdersID,
                    bearerToken: accessToken);

                var merchantOrderDetails = ParseMerchantOrder(merchantOrdersString);

                // Obtener detalles del pago
                var paymentsString = await _GeneralServices.GetAsync(
                    MERCADOPAGO_API_URL_payments,
                    merchantOrderDetails.Split('¯')[11],
                    bearerToken: accessToken);

                var paymentDetails = ParsePaymentDetails(paymentsString);

                // Combinar y almacenar los datos
                var combinedData = merchantOrderDetails + '¯' + paymentDetails;
                var mensaje = await _GeneralServices.ObtenerData("uspInsertarTransaccionesCsv", combinedData);

                //var ExternalReference = combinedData.Split('¯')[2];
                var PaymentID = combinedData.Split('¯')[12];

                if (mensaje.Split('|')[0] == "A")
                {
                    _logger.LogInformation("successful: ", mensaje.Split('|')[1]);
                    return (true, mensaje.Split('|')[1]);
                }
                else
                {
                    _logger.LogInformation("oh no :( : ", mensaje.Split('|')[1]);
                    return (false, mensaje.Split('|')[1]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ProcessTransaction");
                return (false, "Error interno en el controller al procesar la transacción, resource: " + resource);
            }
        }

        private string ParseMerchantOrder(string json)
        {
            var jsonDoc = JsonDocument.Parse(json);
            var rootOrder = jsonDoc.RootElement;

            // Primera mitad de los datos
            long orderId = rootOrder.GetProperty("id").GetInt64();
            string externalReference = rootOrder.GetProperty("external_reference").GetString();
            string orderStatus = rootOrder.GetProperty("status").GetString();
            string orderPaidStatus = rootOrder.GetProperty("order_status").GetString();
            decimal totalAmount = rootOrder.GetProperty("total_amount").GetDecimal();
            decimal paidAmount = rootOrder.GetProperty("paid_amount").GetDecimal();
            decimal refundedAmount = rootOrder.GetProperty("refunded_amount").GetDecimal();
            string dateCreated = rootOrder.GetProperty("date_created").GetString();
            string lastUpdated = rootOrder.GetProperty("last_updated").GetString();
            string notificationUrl = rootOrder.GetProperty("notification_url").GetString();

            // Segunda mitad de los datos
            var payment = rootOrder.GetProperty("payments")[0];
            long paymentId = payment.GetProperty("id").GetInt64();
            decimal transactionAmount = payment.GetProperty("transaction_amount").GetDecimal();
            string paymentStatus = payment.GetProperty("status").GetString();
            string currencyId = payment.GetProperty("currency_id").GetString();
            string dateApproved = payment.GetProperty("date_approved").GetString();

            long payerId = rootOrder.GetProperty("payer").GetProperty("id").GetInt64();

            // Artículos comprados
            var items = rootOrder.GetProperty("items");
            List<string> ids = new List<string>();

            foreach (var item in items.EnumerateArray())
            {
                string productId = item.GetProperty("id").GetString();
                ids.Add(productId);
            }
            string concatenatedIds = string.Join(",", ids);

            // Concatenar datos en dos bloques y unirlos
            string firstHalf = $"{orderId}¯{externalReference}¯{orderStatus}¯{orderPaidStatus}¯" +
                               $"{totalAmount}¯{paidAmount}¯{refundedAmount}¯{currencyId}¯" +
                               $"{dateCreated}¯{lastUpdated}¯{notificationUrl}";

            string secondHalf = $"{paymentId}¯{transactionAmount}¯{paymentStatus}¯{dateApproved}¯" +
                                $"{payerId}¯{concatenatedIds}";

            // Unir ambas mitades
            string result = firstHalf + "¯" + secondHalf;

            return result;
        }

        private string ParsePaymentDetails(string json)
        {
            var jsonDoc = JsonDocument.Parse(json);
            var root = jsonDoc.RootElement;

            // Transacción principal
            var transactionId = root.GetProperty("id").GetInt64();
            var status = root.GetProperty("status").GetString();
            var statusDetail = root.GetProperty("status_detail").GetString();
            var installments = root.GetProperty("installments").GetInt32();
            var operationType = root.GetProperty("operation_type").GetString();
            var paymentMethodId = root.GetProperty("payment_method_id").GetString();
            var paymentTypeId = root.GetProperty("payment_type_id").GetString();
            var statementDescriptor = root.GetProperty("statement_descriptor").GetString();

            // Información de tarjeta
            string firstSixDigits = null, lastFourDigits = null;
            int expirationMonth = 0, expirationYear = 0;
            string cardholderName = null, cardholderIdNumber = null, cardholderIdType = null;

            if (root.TryGetProperty("card", out JsonElement card))
            {
                firstSixDigits = card.TryGetProperty("first_six_digits", out var cardFirstSixDigits) ? cardFirstSixDigits.GetString() : null;
                lastFourDigits = card.TryGetProperty("last_four_digits", out var cardLastFourDigits) ? cardLastFourDigits.GetString() : null;
                expirationMonth = card.TryGetProperty("expiration_month", out var cardExpirationMonth)
                    ? (cardExpirationMonth.ValueKind != JsonValueKind.Null ? cardExpirationMonth.GetInt32() : 0)
                    : 0;

                //expirationYear = card.TryGetProperty("expiration_year", out var cardExpirationYear)
                //    ? cardExpirationYear.GetInt32()
                //    : 0;

                expirationYear = card.TryGetProperty("expiration_year", out var cardExpirationYear)
                ? (cardExpirationYear.ValueKind != JsonValueKind.Null ? cardExpirationYear.GetInt32() : 0)
                    : 0;


                if (card.TryGetProperty("cardholder", out JsonElement cardholder))
                {
                    cardholderName = cardholder.TryGetProperty("name", out var cardholderNameElement) ? cardholderNameElement.GetString() : null;
                    if (cardholder.TryGetProperty("identification", out JsonElement cardholderIdentification))
                    {
                        cardholderIdNumber = cardholderIdentification.TryGetProperty("number", out var idNumber) ? idNumber.GetString() : null;
                        cardholderIdType = cardholderIdentification.TryGetProperty("type", out var idType) ? idType.GetString() : null;
                    }
                }
            }

            // Información del pagador
            var payerEmail = root.GetProperty("payer").GetProperty("email").GetString();
            string payerStreetName = null, payerStreetNumber = null, payerZipCode = null;
            string payerPhoneNumber = null, payerFirstName = null, payerLastName = null;

            if (root.GetProperty("payer").TryGetProperty("address", out JsonElement payerAddress))
            {
                payerStreetName = payerAddress.TryGetProperty("street_name", out var streetName) ? streetName.GetString() : null;
                payerStreetNumber = payerAddress.TryGetProperty("street_number", out var streetNumber) ? streetNumber.GetString() : null;
                payerZipCode = payerAddress.TryGetProperty("zip_code", out var zipCode) ? zipCode.GetString() : null;
            }

            if (root.GetProperty("payer").TryGetProperty("phone", out JsonElement payerPhone))
            {
                payerPhoneNumber = payerPhone.TryGetProperty("number", out var phoneNumber) ? phoneNumber.GetString() : null;
            }

            payerFirstName = root.GetProperty("payer").TryGetProperty("first_name", out var firstName) ? firstName.GetString() : null;
            payerLastName = root.GetProperty("payer").TryGetProperty("last_name", out var lastName) ? lastName.GetString() : null;

            // Detalles de la comisión
            double feeAmount = 0.0;
            string feeType = null;

            if (root.TryGetProperty("fee_details", out JsonElement feeDetailsElement) &&
                feeDetailsElement.ValueKind == JsonValueKind.Array &&
                feeDetailsElement.GetArrayLength() > 0)
            {
                var feeDetails = feeDetailsElement[0];
                feeAmount = feeDetails.GetProperty("amount").GetDouble();
                feeType = feeDetails.GetProperty("type").GetString();
            }

            // Concatenación final
            return string.Join("¯", status, statusDetail, installments, operationType,
                paymentMethodId, paymentTypeId, statementDescriptor, firstSixDigits, lastFourDigits,
                expirationMonth, expirationYear, cardholderName, cardholderIdNumber, cardholderIdType,
                payerEmail, payerStreetName, payerStreetNumber, payerZipCode, payerPhoneNumber,
                payerFirstName, payerLastName, feeAmount, feeType);
        }


        public bool IsValidPayload(JsonElement payload, out string topic, out string resource)
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

        public string ExtractPaymentId(string resource)
        {
            return resource.Split('/').Last();
        }


        public async Task<string> VerificarPaymentExitosoAsync(string external_reference)
        {
            var data = await _GeneralServices.ObtenerData("uspObtenerStatusPaymentCSV", external_reference);

            if (data.StartsWith('¯'))
            {
                return "false";
            }
            else
            {
                var dataPayment = data.Split('¯')[0];
                var dataUsuario = data.Split('¯')[1];


                var isExitodo = dataPayment.Split('|')[1]; // approved
                var accredited = dataPayment.Split('|')[5]; // accredited
                var pago = dataPayment.Split('|')[7]; // paid

                if (isExitodo.Equals("approved") && accredited.Equals("accredited") && pago.Equals("paid"))
                {
                    return "true" + '¯' + dataUsuario;
                }
                else
                {
                    return "false";
                }
            }

        }

    }
}
