using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace OrderService.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException exception)
        {
            await WriteProblemDetailsAsync(context, StatusCodes.Status400BadRequest, "Validation failed.", "One or more validation errors occurred.", exception.Errors.GroupBy(error => error.PropertyName).ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray()));
        }
        catch (KeyNotFoundException exception)
        {
            await WriteProblemDetailsAsync(context, StatusCodes.Status404NotFound, "Resource not found.", exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An unhandled exception occurred while processing {RequestPath}", context.Request.Path);
            await WriteProblemDetailsAsync(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred.", "The server encountered an unexpected condition.");
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, int statusCode, string title, string detail, IReadOnlyDictionary<string, string[]>? errors = null)
    {
        var problemDetails = new ProblemDetails { Status = statusCode, Title = title, Detail = detail, Instance = context.Request.Path };
        if (errors is not null) problemDetails.Extensions["errors"] = errors;
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
