using System.Text.Json;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using ProductService.Api.Middlewares;

namespace ProductService.Tests;

public sealed class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WithValidationException_ReturnsBadRequestProblemDetailsWithErrors()
    {
        // Arrange
        var exception = new ValidationException([new ValidationFailure("Name", "Name is required.")]);
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_ => Task.FromException(exception));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.ContentType.Should().Be("application/problem+json");
        var document = await ReadBodyAsync(context);
        document.RootElement.GetProperty("errors").GetProperty("Name")[0].GetString().Should().Be("Name is required.");
    }

    [Fact]
    public async Task InvokeAsync_WithKeyNotFoundException_ReturnsNotFoundProblemDetails()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_ => Task.FromException(new KeyNotFoundException("Product not found.")));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        var document = await ReadBodyAsync(context);
        document.RootElement.GetProperty("detail").GetString().Should().Be("Product not found.");
    }

    [Fact]
    public async Task InvokeAsync_WithUnhandledException_ReturnsInternalServerErrorProblemDetails()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_ => Task.FromException(new InvalidOperationException("Sensitive detail.")));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        var document = await ReadBodyAsync(context);
        document.RootElement.GetProperty("detail").GetString().Should().NotContain("Sensitive detail.");
    }

    private static ExceptionHandlingMiddleware CreateMiddleware(RequestDelegate next)
    {
        return new ExceptionHandlingMiddleware(next, Mock.Of<ILogger<ExceptionHandlingMiddleware>>());
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/products/1";
        context.Response.Body = new MemoryStream();

        return context;
    }

    private static async Task<JsonDocument> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;

        return await JsonDocument.ParseAsync(context.Response.Body);
    }
}
