using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ValidationException = FluentValidation.ValidationException;
namespace DeliveryPlatform.API.Middleware;

public class ExceptionHandlingMiddlware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddlware> _logger;

    public ExceptionHandlingMiddlware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddlware> logger)
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
        catch (ValidationException ex)
        {
            context.Response.StatusCode =
                StatusCodes.Status400BadRequest;

            context.Response.ContentType = "application/json";

            var errors = ex.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage)
                                  .ToArray());

            var response = new
            {
                StatusCode = 400,
                Message = "Validation failed.",
                Errors = errors
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "A business rule was violated.");

            context.Response.StatusCode =
                StatusCodes.Status409Conflict;

            context.Response.ContentType = "application/json";

            var response = new
            {
                StatusCode = 409,
                Message = ex.Message
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
        catch (UnauthorizedAccessException ex)
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;

            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new
                {
                    StatusCode = 401,
                    Message = ex.Message
                }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred.");

            context.Response.StatusCode =
                StatusCodes.Status500InternalServerError;

            context.Response.ContentType = "application/json";

            var response = new
            {
                StatusCode = 500,
                Message = "An unexpected error occurred."
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}
