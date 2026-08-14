using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZeissAssessment.Domain.Entities;
using ZeissAssessment.Domain.Exceptions;

namespace ZeissAssessment.Api.Infrastructure;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            ProductNotFoundException => (StatusCodes.Status404NotFound, "Product not found"),
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
            InvalidStockOperationException => (StatusCodes.Status409Conflict, "Invalid stock operation"),
            ArgumentOutOfRangeException or ArgumentException => (StatusCodes.Status400BadRequest, "Invalid argument"),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Concurrency conflict"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred"),
        };

        if (status >= 500)
        {
            logger.LogError(exception, "Unhandled exception");
        }
        else
        {
            logger.LogWarning(exception, "Request failed with {Status}", status);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path,
        };

        if (exception is ValidationException ve)
        {
            problem.Extensions["errors"] = ve.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
