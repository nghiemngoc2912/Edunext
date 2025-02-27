using System.Net;
using System.Net.Mail;

namespace Edunext.Helpers
{
    public class EmailHelper
    {
        private readonly IConfiguration _configuration;
        public EmailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var email = _configuration.GetSection("EmailSettings:SenderEmail").Value;
                var password = _configuration.GetSection("EmailSettings:Password").Value;
                var host = _configuration.GetSection("EmailSettings:Host").Value;
                var port = _configuration.GetSection("EmailSettings:Port").Value;
                using (var smtpClient = new SmtpClient(host, int.Parse(port)))
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(email, password);
                    smtpClient.EnableSsl = true;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                    using (var mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress(email);
                        mailMessage.To.Add(toEmail);
                        mailMessage.Subject = subject;
                        mailMessage.Body = body;
                        mailMessage.IsBodyHtml = true;

                        await smtpClient.SendMailAsync(mailMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
