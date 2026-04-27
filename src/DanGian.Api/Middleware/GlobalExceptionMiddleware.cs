using System.Text.Json;
using DanGian.Application.Common;
using DanGian.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DanGian.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, code, message) = exception switch
        {
            NotFoundException ex => (StatusCodes.Status404NotFound, "NOT_FOUND", ex.Message),
            ValidationException ex => (StatusCodes.Status422UnprocessableEntity, "VALIDATION_ERROR",
                string.Join("; ", ex.Errors)),
            DomainException ex => (StatusCodes.Status400BadRequest, "DOMAIN_ERROR", ex.Message),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "UNAUTHORIZED",
                "Authentication required."),
            _ => (StatusCodes.Status500InternalServerError, "INTERNAL_ERROR",
                "An unexpected error occurred."),
        };

        var response = ApiResponse<object>.Fail(code, message);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }));
    }
}
