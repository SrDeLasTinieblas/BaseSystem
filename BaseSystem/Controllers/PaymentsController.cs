using Biblioteca.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BaseSystem.Controllers
{
    public class PaymentsController : ControllerBase
    {
        private readonly GeneralServices _generalServices;
        public PaymentsController(GeneralServices generalServices)
        {
            _generalServices = generalServices;
        }

        [HttpGet("CrearPago")]
        public async Task<IActionResult> CrearPago(string data) // 1|2|1207618741-a817591c-b879-403d-b581-d8c044a88b4a
        {

            var response = await _generalServices.ObtenerData("uspCreatePagoCsv", data);
            var jsonResponse = _generalServices.ConvertToJSON(response);
            string jsonString = JsonConvert.SerializeObject(jsonResponse, Formatting.Indented);

            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return BadRequest("El JsonString estan vacio.");
            }

            return Content(jsonString, "application/json");

        }
    }
}
