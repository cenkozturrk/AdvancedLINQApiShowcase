using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

namespace AdvancedLINQApiShowcase.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // Determine status code
            var statusCode = exception switch
            {
                KeyNotFoundException => HttpStatusCode.NotFound, // 404
                ArgumentException => HttpStatusCode.BadRequest, // 400
                _ => HttpStatusCode.InternalServerError // 500
            };

            // Build error response
            var errorResponse = new
            {
                status = (int)statusCode,
                message = exception.Message,
                details = exception.InnerException?.Message
            };

            context.Response.StatusCode = (int)statusCode;
            return context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    }
}
