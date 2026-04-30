using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace API.Filters
{
    //Customowy filtr porównujący token CSRF zapisany w cookies z tym wysłanym przez request
    public class CsrfFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var request = context.HttpContext.Request;
            
            string? cookieToken = request.Cookies["CSRF-Token"];
            string? headerToken = request.Headers["X-CSRF-Token"];

            headerToken = WebUtility.UrlDecode(headerToken);
            cookieToken = WebUtility.UrlDecode(cookieToken);

            if ((string.IsNullOrEmpty(cookieToken) || string.IsNullOrEmpty(headerToken) || cookieToken != headerToken))
            {
                //W przypadku niezgodności tokenu zwracany jest status code 403
                context.Result = new StatusCodeResult(403);
                return;
            }

            await next();
        }
    }
}
