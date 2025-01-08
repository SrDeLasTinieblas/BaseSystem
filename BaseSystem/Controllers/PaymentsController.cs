using Biblioteca.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BaseSystem.Controllers
{
    public class PaymentsController : ControllerBase
    {
        private readonly MercadoPagoServices _mercadoPagoServices;

        public PaymentsController(MercadoPagoServices mercadoPagoServices)
        {
            _mercadoPagoServices = mercadoPagoServices;
        }

        [HttpGet("CreateCheckoutPreference")]
        public async Task<IActionResult> CrearPago(string data)
        {
            string jsonResponse = await _mercadoPagoServices.CreateCheckoutPreferenceAsync(data);
            return Ok(jsonResponse);
        }

    }
}
