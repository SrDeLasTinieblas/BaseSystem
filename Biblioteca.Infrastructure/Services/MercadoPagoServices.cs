using Biblioteca.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteca.Infrastructure.Services
{
    public class MercadoPagoServices
    {
        private readonly GeneralServices _GeneralServices;
        public MercadoPagoServices(GeneralServices generalServices)
        {
            _GeneralServices = generalServices;
        }

        public async Task<string> CreateCheckoutPreferenceAsync(string data) // idProducto|idUsuario
        {
            // Validar que se recibieron los datos necesarios
            if (string.IsNullOrEmpty(data))
                return "No se proporcionaron los datos necesarios.";

            try
            {
                string[] datos = data.Split('|');
                if (datos.Length < 2)
                    return "Formato de datos inválido. Asegúrate de incluir: idProducto|idUsuario.";

                var DataCheckout = await _GeneralServices.ObtenerData("uspObtenerDataCheckoutCSV", data); // DataCheckout = 120 soles - 4 clases|4|120.00|SamanthaSmith23@yahoo.com

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
                string payerEmail = Usuarios[0];
                string playerName = Usuarios[1];
                string playerSurname = Usuarios[2];
                string player_area_code = Usuarios[3];
                string playerPhone = Usuarios[4];
                string identificationType = Usuarios[5];
                string identificationNumber = Usuarios[6];
                string playerPhoneStreet_name = Usuarios[7];
                string playerPhoneStreet_number = Usuarios[8];
                string playerPhoneZip_code = Usuarios[9];

                // Configuracion
                int payment_methodsInstallments = int.Parse(Configuracion[7]);
                string payment_methodsDefault_payment_method_id = Configuracion[8];
                string notificationUrl = Configuracion[9];
                bool expires = Convert.ToBoolean(Configuracion[10]);
                string expiration_date_from = Configuracion[11];
                string expiration_date_to = Configuracion[12];

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
                        excluded_payment_types = new object[] { },
                        excluded_payment_methods = new object[] { },
                        installments = payment_methodsInstallments, //12,
                        default_payment_method_id = payment_methodsDefault_payment_method_id, //"account_money"
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
                        return responseContent;
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



        //public async Task ProcessWebhookNotification(string rawData)
        //{
        //    try
        //    {
        //        string jsonData = JsonConvert.SerializeObject(rawData);
        //        var parameters = new DynamicParameters();

        //        // Intentar deserializar como notificación base
        //        var baseNotification = JsonConvert.DeserializeObject<MercadoPagoNotificationBase>(jsonData);

        //        if (baseNotification?.Topic == "payment")
        //        {
        //            // Si es una notificación de pago, intentar deserializar como PaymentNotification
        //            var paymentNotification = JsonConvert.DeserializeObject<MercadoPagoPaymentNotification>(jsonData);
        //            if (paymentNotification != null)
        //            {
        //                parameters.Add("@ResourceId", paymentNotification.Data?.Id);
        //                parameters.Add("@Topic", "payment");
        //                parameters.Add("@Action", paymentNotification.Action);
        //                parameters.Add("@ApiVersion", paymentNotification.ApiVersion);
        //                parameters.Add("@PaymentId", paymentNotification.Data?.Id);
        //                parameters.Add("@DateCreated", paymentNotification.DateCreated);
        //                parameters.Add("@NotificationId_MP", paymentNotification.Id);
        //                parameters.Add("@LiveMode", paymentNotification.LiveMode);
        //                parameters.Add("@Type", paymentNotification.Type);
        //                parameters.Add("@UserId", paymentNotification.UserId);
        //            }
        //        }
        //        else if (baseNotification?.Topic == "merchant_order")
        //        {
        //            parameters.Add("@ResourceUrl", baseNotification.Resource);
        //            parameters.Add("@Topic", "merchant_order");

        //            // Extraer el ID del merchant_order de la URL
        //            var merchantOrderId = baseNotification.Resource.Split('/').LastOrDefault();
        //            parameters.Add("@ResourceId", merchantOrderId);
        //        }

        //        parameters.Add("@RawData", jsonData);

        //        // Guardar en la base de datos
        //        await _db.ExecuteAsync("uspInsertMPNotification", parameters, commandType: CommandType.StoredProcedure);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error processing MercadoPago webhook notification");
        //        throw;
        //    }
        //}

    }
}
