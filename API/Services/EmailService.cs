using Resend;
using System.Net;
using System.Net.Mail;

namespace API.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmail(string r, string t);
    }

    public class EmailService (IConfiguration configuration) : IEmailService
    {
        private readonly string _resendApiKey = configuration["ResendApiKey"]!;
        private readonly string _apiAddress = configuration["Issuer"]!;

        public async Task<bool> SendEmail(string recipent, string token)
        {
            try
            {
                string verificationLink = _apiAddress + "/login/verify-email?token=" + token;
                var html = File.ReadAllText("EmailTemplates\\verification.html").Replace("{{verificationLink}}", verificationLink);
                
                IResend resend = ResendClient.Create(_resendApiKey);

                var resp = await resend.EmailSendAsync(new EmailMessage()
                {
                    From = "onboarding@resend.dev",
                    To = recipent,
                    Subject = "Weryfikacja",
                    HtmlBody = html,
                });

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
    }
}
