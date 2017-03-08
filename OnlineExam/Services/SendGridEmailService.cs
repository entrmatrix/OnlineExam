using System;
using System.Collections.Generic;
using System.Linq;

using System.Net.Mail;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.Web;
using System.Threading.Tasks;

namespace OnlineExam.Services
{
    public class SendGridEmailService
    {
        private readonly string sendGridApiKey;
        private string SendGridApiKey { get { return sendGridApiKey; } }

        public string Sender { get; set; }
        public string SenderName { get; set; }

        public SendGridEmailService(string key, string sender, string senderName)
        {
            this.sendGridApiKey = key;
            Sender = sender;
            SenderName = senderName;

        }

        public async Task SendAsync(string subject, string destination, string htmlbody, string textBody)
        {
            var client = new SendGridClient(SendGridApiKey);
            var from = new EmailAddress(Sender, SenderName);
            var to = new EmailAddress(destination);
            var plainTextContent = textBody;
            var htmlContent = htmlbody;

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);


        }

      



    }

}