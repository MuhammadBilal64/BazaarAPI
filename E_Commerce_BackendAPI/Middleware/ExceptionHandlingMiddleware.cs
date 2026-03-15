using System.Net;
using System.Text.Json;
using E_Commerce_BackendAPI.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_BackendAPI.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, title, detail) = exception switch
            {
                NotFoundException nf => (HttpStatusCode.NotFound, "Not Found", nf.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized", exception.Message),
                ArgumentException arg => (HttpStatusCode.BadRequest, "Bad Request", arg.Message),
                InvalidOperationException inv => (HttpStatusCode.Conflict, "Conflict", inv.Message),
                _ => (HttpStatusCode.InternalServerError, "An error occurred", "An unexpected error occurred. Please try again later.")
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;

            var problemDetails = new ProblemDetails
            {
                Type = $"https://tools.ietf.org/html/rfc9110#section-15.5.{(int)statusCode}",
                Title = title,
                Status = (int)statusCode,
                Detail = detail,
                Instance = context.Request.Path,
                Extensions = { ["traceId"] = context.TraceIdentifier }
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, options));
        }
    }
}
