using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace GalleryDatabase.Services
{
    public class EmailSender : IEmailSender
    {
        public IConfiguration _configuration { get; protected set; }

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string text)
        {
            string recipient = email;
            string sender = _configuration["EmailSender:Sender"]; 

            MailMessage message = new MailMessage(sender, recipient);

            message.Subject = subject;
            message.Body = text;
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;

            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);  
            NetworkCredential basicCredential1 = new NetworkCredential(sender, _configuration["EmailSender:RandomGeneratedAppToken"]);

            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = basicCredential1;

            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
