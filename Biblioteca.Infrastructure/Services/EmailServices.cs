using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Metadata;

namespace Biblioteca.Infrastructure.Services
{
    public class EmailServices
    {
        private readonly GeneralServices _generalServices;
        private readonly SmtpClient _smtpClient;
        private readonly string _fromAddress;

        public EmailServices(GeneralServices generalServices)
        {
            _generalServices = generalServices;

            // Inicializar configuración SMTP
            var dataEmailSetting = _generalServices.ObtenerData("uspObtenerDataEmailCSV", "")
                .GetAwaiter()
                .GetResult();

            var settingParts = dataEmailSetting.Split('|');

            if (settingParts.Length != 4)
            {
                throw new InvalidOperationException("La configuración del email no tiene el formato correcto");
            }

            string smtpServer = settingParts[0];
            int port = Convert.ToInt32(settingParts[1]);
            _fromAddress = settingParts[2];
            string password = settingParts[3];

            _smtpClient = new SmtpClient(smtpServer, port)
            {
                Credentials = new NetworkCredential(_fromAddress, password),
                EnableSsl = true
            };
        }


        public async Task SendVerificationEmail(string toEmail, string subject, string verificationCode)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromAddress),
                Subject = subject, // asunto
                Body = $"<p>Tu código de verificación es: <strong>{verificationCode}</strong></p>",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toEmail);

            try
            {
                await _smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar el correo: {ex.Message}");
                throw;
            }
        }

        public string GenerateVerificationCode()
        {
            Random random = new Random();
            var code = random.Next(1000, 9999).ToString();
            return code;

        }



    }
}
