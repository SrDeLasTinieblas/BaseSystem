using Biblioteca.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
                //[uspObtenerDataCheckoutCSV]

                string[] datos = data.Split('|');
                if (datos.Length < 2)
                    return "Formato de datos inválido. Asegúrate de incluir: idProducto|idUsuario.";

                var DataCheckout = await _GeneralServices.ObtenerData("uspObtenerDataCheckoutCSV", data); // DataCheckout = 120 soles - 4 clases|4|120.00|SamanthaSmith23@yahoo.com

                string[] DatosCheckout = DataCheckout.Split('|');
                //if (DataCheckout.Length < 4)
                //    return "Formato de datos inválido. Asegúrate de incluir: title|quantity|unit_price|email en el retorno de uspObtenerDataCheckoutCSV.";

                string autoReturn = DatosCheckout[0];
                string back_urls_success = DatosCheckout[1];
                string back_urls_failure = DatosCheckout[2];
                string back_urls_pending = DatosCheckout[3];
                string statementDescriptor = DatosCheckout[4];
                string binaryMode = DatosCheckout[5];
                string externalReference = DatosCheckout[6];
                string ID = DatosCheckout[7];
                string title = DatosCheckout[8];
                int quantity = int.Parse(DatosCheckout[9]);
                decimal unitPrice = decimal.Parse(DatosCheckout[10]);
                string Descripcion = DatosCheckout[11];
                string categoryId = DatosCheckout[12];

                string payerEmail = DatosCheckout[13];
                string playerName = DatosCheckout[14];
                string playerSurname = DatosCheckout[15];
                string player_area_code = DatosCheckout[16];
                string playerPhone = DatosCheckout[17];

                string identificationType = DatosCheckout[18];
                string identificationNumber = DatosCheckout[19];
                string address = DatosCheckout[20];
                string playerPhoneStreet_name = DatosCheckout[21];
                string playerPhoneStreet_number = DatosCheckout[22];
                string playerPhoneZip_code = DatosCheckout[23];

                int payment_methodsInstallments = int.Parse(DatosCheckout[24]);
                string payment_methodsDefault_payment_method_id = DatosCheckout[25];
                string notificationUrl = DatosCheckout[26];
                string expires = DatosCheckout[27];
                string expiration_date_from = DatosCheckout[28];
                string expiration_date_to = DatosCheckout[29];

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
                    items = new[]
                    {
                        new
                        {
                            id = ID, //"010983098",
                            title = title,
                            quantity = quantity,
                            unit_price = unitPrice,
                            description = Descripcion,
                            category_id = categoryId, //"retail"
                        }
                    },
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




    }
}
