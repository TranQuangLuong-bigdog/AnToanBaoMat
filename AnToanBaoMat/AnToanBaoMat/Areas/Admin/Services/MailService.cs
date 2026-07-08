using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace AnToanBaoMat.Services
{
    public class MailService
    {
        private readonly IConfiguration _config;

        public MailService(IConfiguration config)
        {
            _config = config;
        }

        public void SendEmail(
            string to,
            string subject,
            string body)
        {
            var message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(
                    "CV SAFE",
                    _config["EmailSettings:Email"]));

            message.To.Add(
                MailboxAddress.Parse(to));

            message.Subject = subject;

            message.Body = new TextPart("html")
            {
                Text = body
            };

            using var client = new SmtpClient();

            client.ServerCertificateValidationCallback =
                (sender, certificate, chain, errors) => true;

            client.Connect(
                _config["EmailSettings:Host"],
                int.Parse(_config["EmailSettings:Port"]),
                MailKit.Security.SecureSocketOptions.StartTls);

            client.Authenticate(
                _config["EmailSettings:Email"],
                _config["EmailSettings:Password"]);

            client.Send(message);


            client.Disconnect(true);
        }
    }
}