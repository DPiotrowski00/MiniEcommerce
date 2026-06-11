using System.Net.Mail;

namespace API.Middleware
{
    public class LoggingMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                string logDir = Path.Combine(AppContext.BaseDirectory, "ErrorLog");

                string filePath = Path.Combine(
                    logDir,
                    $"error-log-{DateTime.Today:yyyy-MM-dd}.txt"
                );

                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                using var file = File.Open(filePath, FileMode.Append);

                using var writer = new StreamWriter(file);
                await writer.WriteLineAsync($"{DateTime.Now:HH:mm:ss}");
                await writer.WriteLineAsync("--------");
                await writer.WriteLineAsync(ex.ToString());

                Console.WriteLine("An error has been logged.");

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("Internal Server Error");
            }
        }
    }
}
