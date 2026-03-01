using System.Net;
using System.Text.Json;
using Application.Common.Exceptions;

namespace API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            ValidationException ve => (HttpStatusCode.BadRequest, "Validation failed", ve.Errors),
            NotFoundException nfe => (HttpStatusCode.NotFound, nfe.Message, (IDictionary<string, string[]>?)null),
            ForbiddenException => (HttpStatusCode.Forbidden, "Forbidden", null),
            Domain.Exceptions.DomainException de => (HttpStatusCode.BadRequest, de.Message, null),
            _ => (HttpStatusCode.InternalServerError, "An internal error occurred.", null)
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new ErrorResponse
        {
            Status = (int)statusCode,
            Message = message,
            Errors = errors,
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}

public class ErrorResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = null!;
    public IDictionary<string, string[]>? Errors { get; set; }
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; }
}
