using Biblioteca.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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

        public async Task<string> CreatePaymentPreference(string data)
        {
            var obtenerHash = await _GeneralServices.ObtenerData("uspAutenticacionCsv", data);

            if (string.IsNullOrEmpty(obtenerHash))
                return "No obtuvo ningun dato de uspAutenticacionCsv.";

            string[] datos = data.Split("|");
            data = datos[0] + '|' + obtenerHash + '|' + _GeneralServices.GetClientIP();

            var response = await _GeneralServices.ObtenerData("uspLoginCsv", data);

            string passBD = "";
            string rolBD = "";
            int DuracionTokenSesion = 5;

            var dataParts = data.Split('|');
            var emailInput = dataParts[0];
            var textPassword = datos[1];

            try
            {
                var resultPart = response.Split("¯");
                var responseData = resultPart[0].Split("¬");
                var resultMessage = resultPart[1].Split("|");

                string res = resultMessage[0];
                string messageBD = resultMessage[1];

                if (res == "A")
                {
                    var userData = responseData[3].Split("|");

                    passBD = userData[2];
                    rolBD = userData[3];
                    DuracionTokenSesion = Convert.ToInt32(userData[4]);
                }
                else if (res == "E")
                {
                    return messageBD;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error procesando la respuesta", ex);
            }

            if (!BCrypt.Net.BCrypt.Verify(textPassword, passBD))
            {
                return ("E|La contraseña no coincide");
            }


            return "token";
        }

    }
}
