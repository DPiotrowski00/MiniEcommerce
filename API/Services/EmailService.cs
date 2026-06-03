using API.DataModels;
using Resend;
using System.Net;
using System.Net.Mail;

namespace API.Services
{
    public interface IEmailService
    {
        Task<bool> SendVerificationEmail(string r, string t);
        Task<bool> SendOrderConfirmation(OrderModel order);
    }

    public class EmailService (IConfiguration configuration, IItemsSqlService itemsSqlService, ILoggingSqlService loggingSqlService) : IEmailService
    {
        private readonly string _resendApiKey = configuration["ResendApiKey"]!;
        private readonly string _apiAddress = configuration["Issuer"]!;
        private readonly string _frontAddress = configuration["Front"]!;

        private readonly IItemsSqlService _itemsSqlService = itemsSqlService;
        private readonly ILoggingSqlService _loggingSqlService = loggingSqlService;

        public async Task<bool> SendVerificationEmail(string recipent, string token)
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

        public async Task<bool> SendOrderConfirmation(OrderModel order)
        {
            try
            {
                UserModel user = await _loggingSqlService.GetUser(order.UserID);
                
                decimal totalValue = 0;
                foreach (var position in order.Positions)
                {
                    var item = await _itemsSqlService.GetItemById(position.ItemID);
                    totalValue += item.Price * position.Quantity;
                }
                
                string orderLink = _frontAddress + "/order?id=" + order.ID;
                var html = File.ReadAllText("EmailTemplates\\order_confirmation.html")
                .Replace("{{orderLink}}", orderLink)
                .Replace("{{orderNumber}}", order.ID.ToString())
                .Replace("{{orderTotal}}", $"{totalValue:F2} PLN")
                .Replace("{{customerName}}", user.DisplayName);

                IResend resend = ResendClient.Create(_resendApiKey);
                var resp = await resend.EmailSendAsync(new EmailMessage()
                {
                    From = "onboarding@resend.dev",
                    To = user.Email,
                    Subject = "Potwierdzenie zamówienia",
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
