using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteca.Infrastructure.Services
{
    public class EmailServices
    {

        private readonly SmtpClient _smtpClient;
        private readonly string _fromAddress;

        public EmailServices(string smtpServer, int port, string fromAddress, string fromPassword)
        {
            _smtpClient = new SmtpClient(smtpServer, port)
            {
                Credentials = new NetworkCredential(fromAddress, fromPassword),
                EnableSsl = true
            };
            _fromAddress = fromAddress;
        }

        public async Task SendVerificationEmail(string toEmail, string Subject, string verificationCode)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromAddress),
                Subject = Subject, //"Verificación de correo electrónico",
                //Body = $"<p>Gracias por registrarte. Tu código de verificación es: <strong>{verificationCode}</strong></p>",
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
                // Manejo de errores
                Console.WriteLine($"Error al enviar el correo: {ex.Message}");
                throw; // Lanzar la excepción para manejarla en niveles superiores si es necesario
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
