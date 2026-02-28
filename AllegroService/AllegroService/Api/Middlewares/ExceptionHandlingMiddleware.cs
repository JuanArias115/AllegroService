using System.Net;
using AllegroService.Application.Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AllegroService.Api.Middlewares;

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
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail, extensions) = exception switch
        {
            ValidationException validationException =>
                (StatusCodes.Status400BadRequest, "Validation error", "One or more validation errors occurred.",
                    new Dictionary<string, object?> { ["errors"] = validationException.Errors.Select(x => x.ErrorMessage).ToArray() }),
            ForbiddenException =>
                (StatusCodes.Status403Forbidden, "Forbidden", exception.Message, null),
            UnauthorizedAccessException =>
                (StatusCodes.Status401Unauthorized, "Unauthorized", exception.Message, null),
            KeyNotFoundException =>
                (StatusCodes.Status404NotFound, "Not found", exception.Message, null),
            _ =>
                (StatusCodes.Status500InternalServerError, "Server error", "An unexpected error occurred.", null)
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = $"https://httpstatuses.com/{statusCode}",
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        if (extensions is not null)
        {
            foreach (var extension in extensions)
            {
                problemDetails.Extensions[extension.Key] = extension.Value;
            }
        }

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
