using BCrypt.Net;
using Biblioteca.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteca.Infrastructure.Services
{
    public class UsuarioServices
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly GeneralServices _GeneralServices;
        private readonly JWTServices _jWTServices;

        public UsuarioServices(AppDbContext context, IConfiguration configuration, GeneralServices generalServices, JWTServices jWTServices)
        {
            _context = context;
            _configuration = configuration;
            _GeneralServices = generalServices;
            _jWTServices = jWTServices;
        }

        public async Task<string> AuthenticateUsuario(string data) // StevenLee893@live.com|PassTextPlain
        {
            // Busco mediante el correo la contraseña hasheada que esta en la base de datos
            var obtenerHash = await _GeneralServices.ObtenerData("uspAutenticacionCsv", data); // obtenerHash = $2a$11$n9C1TzjBNcdd6ql57B3pq.2PQhOK3cYQQjRagVC7HZ5jMJSC5YDj2

            if (string.IsNullOrEmpty(obtenerHash))
                return "No obtuvo ningun dato de uspAutenticacionCsv.";


            string[] datos = data.Split("|"); // datos[0] JhonTheRipper@gmail.com datos[1] = PassTextPlain
            data = datos[0] + '|' + obtenerHash + '|' + _GeneralServices.GetClientIP();

            var response = await _GeneralServices.ObtenerData("uspLoginCsv", data); // response = $2a$11$n9C1TzjBNcdd6ql57B3pq.2PQhOK3cYQQjRagVC7HZ5jMJSC5YDj2

            string passBD = "";
            string rolBD = "";
            int DuracionTokenSesion = 5;

            var dataParts = data.Split('|');
            var emailInput = dataParts[0];
            var textPassword = datos[1]; // PassTextPlain

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

            var token = _jWTServices.GenerateJwtToken(emailInput, rolBD, DuracionTokenSesion);
            return token;
        }

        public async Task<string> RegisterUsuario(string data) // JhonTheRipper3@gmail.com|pass123
        {
            var datos = data.Split("|");
            string password = datos[2];

            var PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            datos[2] = PasswordHash;
            string dataConHash = string.Join("|", datos);

            var response = await _GeneralServices.ObtenerData("uspRegisterUsuarioCsv", dataConHash);

            return response;
        }




    }
}
