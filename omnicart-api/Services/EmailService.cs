
using System.Net;
using System.Net.Mail;

namespace omnicart_api.Services
{
    public class EmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;

        public EmailService()
        {
            _smtpHost = "smtp.yourprovider.com";
            _smtpPort = 587;
            _smtpUser = "email";
            _smtpPass = "password";
        }

        public async Task SendPasswordResetAsync(string email, string resetToken)
        {
            var baseUrl = "https://omnicart.com";
            var resetUrl = $"{baseUrl}/reset-password?token={resetToken}";

            using (var smtpClient = new SmtpClient(_smtpHost, _smtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
                smtpClient.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpUser, "Omnicart Support"),
                    Subject = "Password Reset Request",
                    Body = $"<p>Please use the following link to reset your password: </p><p><a href='{resetUrl}'>Reset Password</a></p>",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                try
                {
                    await smtpClient.SendMailAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to send email.", ex);
                }
            }
        }
    }
}
