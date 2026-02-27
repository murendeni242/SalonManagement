using System.Net;
using System.Text.Json;
using Salon.Domain.Common;

namespace Salon.API.Middleware;

/// <summary>
/// Global exception handler. Sits in the middleware pipeline and catches every
/// unhandled exception before it reaches the client.
///
/// Maps known exception types to the correct HTTP status codes so the frontend
/// always receives a structured, predictable error response — never a raw 500 stack trace.
///
/// Response shape (consistent across ALL error responses):
/// {
///   "error": "Human-readable message the frontend can show directly.",
///   "code":  "MACHINE_READABLE_CODE"
/// }
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, errorCode, message) = exception switch
        {
            // Business rule violated — show the message directly to the user
            DomainException domainEx =>
                (HttpStatusCode.BadRequest, "DOMAIN_RULE_VIOLATED", domainEx.Message),

            // Entity not found — 404
            NotFoundException notFoundEx =>
                (HttpStatusCode.NotFound, "NOT_FOUND", notFoundEx.Message),

            // Duplicate email, invalid credentials, etc — 400
            ApplicationException appEx =>
                (HttpStatusCode.BadRequest, "BAD_REQUEST", appEx.Message),

            // Anything else — 500, hide details in production
            _ => (
                HttpStatusCode.InternalServerError,
                "INTERNAL_ERROR",
                _env.IsDevelopment()
                    ? exception.Message                         // show detail in dev
                    : "An unexpected error occurred. Please try again.")  // hide in prod
        };

        context.Response.StatusCode = (int)statusCode;

        var response = JsonSerializer.Serialize(
            new { error = message, code = errorCode },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        return context.Response.WriteAsync(response);
    }
}