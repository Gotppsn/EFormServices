// EFormServices.Web/Middleware/ExceptionMiddleware.cs
// Got code 30/05/2025
using EFormServices.Application.Common.Exceptions;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace EFormServices.Web.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred");
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (status, error) = exception switch
        {
            Application.Common.Exceptions.ValidationException ex => (HttpStatusCode.BadRequest, CreateValidationErrorResponse(ex)),
            NotFoundException => (HttpStatusCode.NotFound, CreateErrorResponse("Resource not found", exception.Message)),
            ForbiddenAccessException => (HttpStatusCode.Forbidden, CreateErrorResponse("Access denied", exception.Message)),
            ConflictException => (HttpStatusCode.Conflict, CreateErrorResponse("Conflict", exception.Message)),
            FluentValidation.ValidationException ex => (HttpStatusCode.BadRequest, CreateFluentValidationErrorResponse(ex)),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, CreateErrorResponse("Unauthorized", "Authentication required")),
            _ => (HttpStatusCode.InternalServerError, CreateErrorResponse("Internal server error", "An error occurred while processing your request"))
        };

        context.Response.StatusCode = (int)status;
        await context.Response.WriteAsync(JsonSerializer.Serialize(error));
    }

    private static object CreateValidationErrorResponse(Application.Common.Exceptions.ValidationException ex)
    {
        return new
        {
            type = "validation_error",
            title = "Validation failed",
            status = 400,
            errors = ex.Errors
        };
    }

    private static object CreateFluentValidationErrorResponse(FluentValidation.ValidationException ex)
    {
        var errors = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return new
        {
            type = "validation_error",
            title = "Validation failed",
            status = 400,
            errors
        };
    }

    private static object CreateErrorResponse(string title, string detail)
    {
        return new
        {
            type = "error",
            title,
            detail,
            status = 500
        };
    }
}