using FluentValidation;

namespace ZeissAssessment.Api.Infrastructure;

/// <summary>
/// MVC filter-style validation via manual pipeline: FluentValidation.AspNetCore integration
/// automatically validates incoming requests when validators are registered in DI.
/// This class provides an explicit action filter as an additional safeguard.
/// </summary>
public class FluentValidationFilter(IServiceProvider serviceProvider)
    : Microsoft.AspNetCore.Mvc.Filters.IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context,
        Microsoft.AspNetCore.Mvc.Filters.ActionExecutionDelegate next)
    {
        foreach (var arg in context.ActionArguments.Values.Where(v => v is not null))
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(arg!.GetType());
            if (serviceProvider.GetService(validatorType) is not IValidator validator) continue;

            var validationContext = new ValidationContext<object>(arg);
            var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);
            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }

        await next();
    }
}
