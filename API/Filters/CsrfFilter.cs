using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace API.Filters
{
    //Customowy filtr porównujący token CSRF zapisany w cookies z tym wysłanym przez request
    public class CsrfFilter(ISessionSqlService sessionSqlService) : IAsyncActionFilter
    {
        private readonly ISessionSqlService _sessionSqlService = sessionSqlService;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var request = context.HttpContext.Request;

            var sid = context.HttpContext.User.FindFirst("sid")?.Value;

            string? headerToken = request.Headers["X-CSRF-Token"];
            string? expectedToken = await _sessionSqlService.GetExpectedToken(Convert.ToInt32(sid));

            headerToken = WebUtility.UrlDecode(headerToken);

            Console.WriteLine($"Expected Token: {expectedToken}");
            Console.WriteLine($"Header Token: {headerToken}");

            if ((string.IsNullOrEmpty(expectedToken) || string.IsNullOrEmpty(headerToken) || expectedToken != headerToken))
            {
                //W przypadku niezgodności tokenu zwracany jest status code 403
                context.Result = new StatusCodeResult(403);
                return;
            }

            await next();
        }
    }
}
