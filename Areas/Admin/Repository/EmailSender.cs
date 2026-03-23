using System.Net;
using System.Net.Mail;

namespace buytoy.Areas.Admin.Repository
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true, //bật bảo mật
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("0903682500a@gmail.com", "vdyxkhecxelzngpp")
            };

            return client.SendMailAsync(
                new MailMessage(from: "0903682500a@gmail.com",
                                to: email,
                                subject,
                                message
                                ));
        }
    }
}
