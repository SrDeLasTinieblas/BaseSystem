using Biblioteca.Infrastructure.Persistence;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

using System.Text;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Net.Http;

namespace Biblioteca.Infrastructure.Services
{
    public class GeneralServices
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;

        public GeneralServices(AppDbContext context, IHttpContextAccessor httpContextAccessor, HttpClient httpClient)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient ?? new HttpClient();

        }

        public async Task<string> ObtenerData(string nameProcedure, string dataParameter)
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                try
                {
                    using var command = connection.CreateCommand();
                    command.CommandText = nameProcedure;
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    var param = command.CreateParameter();
                    param.ParameterName = "@data";
                    param.Value = dataParameter;
                    command.Parameters.Add(param);

                    using var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        return reader.GetString(0);
                    }
                    return string.Empty;
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ObtenerData: {ex}");
                throw new Exception($"Error al obtener datos para {nameProcedure}", ex);
            }
        }

        public JArray ConvertToJSON(string response)
        {
            var parts = response.Split('°');
            var headers = parts[0].Split('|');
            var types = parts[1].Split('|');
            var data = parts[2].Split('¬');

            var jsonArray = new JArray();

            foreach (var row in data)
            {
                var values = row.Split('|');

                var jsonObject = new JObject();

                for (int i = 0; i < headers.Length && i < values.Length; i++)
                {
                    string value = values[i];
                    string type = types[i];

                    JToken token;
                    switch (type.ToLower())
                    {
                        case "int32":
                            token = int.TryParse(value, out int intValue) ? new JValue(intValue) : new JValue(value);
                            break;
                        case "datetime":
                            token = DateTime.TryParse(value, out DateTime dateValue) ? new JValue(dateValue) : new JValue(value); // format: yy-mm-dd:hh-mm-ss
                            break;
                        case "time":
                            token = TimeSpan.TryParse(value, out TimeSpan timeValue) ? new JValue(timeValue) : new JValue(value); // format: hh-mm-ss:ms
                            break;
                        case "string":
                            token = new JValue(value);
                            break;
                        case "int64":
                            token = long.TryParse(value, out long longValue) ? new JValue(longValue) : new JValue(value);
                            break;
                        case "double":
                            token = double.TryParse(value, out double doubleValue) ? new JValue(doubleValue) : new JValue(value); // Para hacer calculos cientificos 
                            break;
                        case "decimal":
                            token = decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalValue)
                                ? new JValue(decimalValue)
                                : new JValue(value); // Para hacer cálculos monetarios
                            break;
                        case "boolean":
                            token = bool.TryParse(value, out bool boolValue) ? new JValue(boolValue) : new JValue(value);
                            break;
                        case "float":
                            token = float.TryParse(value, out float floatValue) ? new JValue(floatValue) : new JValue(value);
                            break;
                        case "guid":
                            token = Guid.TryParse(value, out Guid guidValue) ? new JValue(guidValue) : new JValue(value); // ejemplo: "IDUser": "d9b1d7db-5dd7-4f6d-b3ae-8e6c237d6700"
                            break;
                        default:
                            token = new JValue(value);
                            break;
                    }

                    jsonObject[headers[i]] = token;
                }

                jsonArray.Add(jsonObject);
            }

            return jsonArray;
        }

        public string GetClientIP()
        {
            var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            var forwardedIp = _httpContextAccessor.HttpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            return forwardedIp ?? ip ?? "IP no disponible";
        }



        public async Task<string> GetAsync(
            string baseUrl,
            string resourceId,
            Dictionary<string, string> parameters = null,
            string bearerToken = null)
        {
            try
            {
                // Construir la URL completa
                var url = $"{baseUrl}{resourceId}";

                // Añadir parámetros de consulta si existen
                if (parameters?.Count > 0)
                {
                    var queryString = string.Join("&", parameters.Select(kvp =>
                        $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
                    url = $"{url}?{queryString}";
                }

                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    // Configurar el token de autorización
                    if (!string.IsNullOrWhiteSpace(bearerToken))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                    }

                    // Realizar la solicitud
                    var response = await _httpClient.SendAsync(request);

                    // Verificar la respuesta
                    response.EnsureSuccessStatusCode();

                    // Leer la respuesta como string
                    var content = await response.Content.ReadAsStringAsync();

                    // Retornar la respuesta como string
                    return content;
                }
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Error en la solicitud HTTP: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inesperado durante la solicitud GET: {ex.Message}", ex);
            }
        }



    }
}
