using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System.Collections.Generic;
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
            /* vytvoření zprávy header + body */
            
            var message = new Message
            {
                Subject = subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = text
                },
                ToRecipients = new List<Recipient>()
                {
                    new Recipient { EmailAddress = new EmailAddress { Address = email } }
                }
            };
            /* vytvoření MS Graph API spojení */
            string[] scopes = new string[] { "https://graph.microsoft.com/.default" };
            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
              .Create(_configuration["EmailSender:ClientId"])
              .WithTenantId(_configuration["EmailSender:TenantId"])
              .WithClientSecret(_configuration["EmailSender:ClientSecret"])
              .Build();
            await confidentialClientApplication.AcquireTokenForClient(scopes)
              .ExecuteAsync()
              .ConfigureAwait(false);
            var authProvider = new ClientCredentialProvider(confidentialClientApplication);
            var graphClient = new GraphServiceClient(authProvider);
            /* odeslání zprávy prostřednictvím Grap API Mail.Send */
            await graphClient.Users[_configuration["EmailSender:UserId"]]
              .SendMail(message, false)
              .Request()
              .PostAsync();
        }
    }
}
