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
        /*
        [HttpPost("ConfirmarPago")]
        public async Task<IActionResult> ConfirmarPago(string data)
        {
            // luego se llama a esta api para completar el pago
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var resultado = await _ImercadoLibreService.CrearPreferenciaPago(
                request.Titulo,
                request.Precio,
                request.Email,
                request.AlumnoId,
                request.SelectedPackageId,
                request.PaymentId,
                token = "TEST-4688634476959666-093023-acd10d00061994ee3d4b2007de6f5c66-1207618741"
                );

            if (string.IsNullOrWhiteSpace(resultado))
            {
                return BadRequest("Error al crear la preferencia de pago.");
            }
            PaymentPreference paymentPreference = JsonConvert.DeserializeObject<PaymentPreference>(resultado);
            return Ok(paymentPreference);
        }
        */

    }
}
