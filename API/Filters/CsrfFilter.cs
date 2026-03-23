using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace API.Filters
{
    public class CsrfFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var request = context.HttpContext.Request;
            
            string? cookieToken = request.Cookies["CSRF_Token"];
            string? headerToken = request.Headers["X-CSRF-Token"];

            headerToken = WebUtility.UrlDecode(headerToken);

            if ((string.IsNullOrEmpty(cookieToken) || string.IsNullOrEmpty(headerToken) || cookieToken != headerToken))
            {
                context.Result = new StatusCodeResult(403);
                return;
            }

            await next();
        }
    }
}
