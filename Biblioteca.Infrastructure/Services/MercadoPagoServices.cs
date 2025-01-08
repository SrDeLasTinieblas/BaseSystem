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

        public async Task<string> CreateCheckoutPreferenceAsync(string data)
        {
            // Validar que se recibieron los datos necesarios
            if (string.IsNullOrEmpty(data))
                return "No se proporcionaron los datos necesarios.";

            try
            {
                // Extraer datos requeridos desde el string
                string[] datos = data.Split('|');
                if (datos.Length < 4)
                    return "Formato de datos inválido. Asegúrate de incluir: title|quantity|unit_price|email.";

                string title = datos[0];
                int quantity = int.Parse(datos[1]);
                decimal unitPrice = decimal.Parse(datos[2]);
                string payerEmail = datos[3];

                // Crear la carga útil (payload) para el request
                var payload = new
                {
                    auto_return = "approved",
                    back_urls = new
                    {
                        success = "http://httpbin.org/get?back_url=success",
                        failure = "http://httpbin.org/get?back_url=failure",
                        pending = "http://httpbin.org/get?back_url=pending"
                    },
                    statement_descriptor = "TestStore",
                    binary_mode = false,
                    external_reference = "IWD1238971",
                    items = new[]
                    {
                new
                {
                    id = "010983098",
                    title = title,
                    quantity = quantity,
                    unit_price = unitPrice,
                    description = "Description of my product",
                    category_id = "retail"
                }
            },
                    payer = new
                    {
                        email = payerEmail,
                        name = "Juan",
                        surname = "Lopez",
                        phone = new
                        {
                            area_code = "11",
                            number = "1523164589"
                        },
                        identification = new
                        {
                            type = "DNI",
                            number = "12345678"
                        },
                        address = new
                        {
                            street_name = "Street",
                            street_number = 123,
                            zip_code = "1406"
                        }
                    },
                    payment_methods = new
                    {
                        excluded_payment_types = new object[] { },
                        excluded_payment_methods = new object[] { },
                        installments = 12,
                        default_payment_method_id = "account_money"
                    },
                    notification_url = "https://www.your-site.com/webhook",
                    expires = true,
                    expiration_date_from = "2024-01-01T12:00:00.000-04:00",
                    expiration_date_to = "2025-12-31T12:00:00.000-04:00"
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
