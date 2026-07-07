using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace VanDerTil.ContosoUniversity.Web.Infrastructure.Filters;

/// <summary>
/// An action filter that uses FluentValidation to validate action parameters.
/// </summary>
public sealed class FluentValidationActionFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public FluentValidationActionFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var arguments = context.ActionArguments;

        object? lastModel = null;
        if (arguments.Count > 0)
        {
            foreach (var argument in arguments.Values)
            {
                if (argument is null)
                {
                    continue;
                }

                // We are not interested in validating these kind of arguments.
                var argumentType = argument.GetType();
                if (argumentType.IsPrimitive
                    || argumentType == typeof(string)
                    || argumentType == typeof(CancellationToken))
                {
                    continue;
                }

                var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
                var validator = (IValidator?)_serviceProvider.GetService(validatorType);

                if (validator != null)
                {
                    // The ValidationContext<object> does not look nice, but it works.
                    var result = await validator.ValidateAsync(new ValidationContext<object>(argument));

                    AddToModelState(result, context.ModelState);
                    lastModel = argument;
                }
            }
        }

        await next();
    }

    /// <summary>
    /// Stores the errors in a ValidationResult object to the specified modelstate dictionary.
    /// </summary>
    /// <param name="result">The validation result to store</param>
    /// <param name="modelState">The ModelStateDictionary to store the errors in.</param>
    private static void AddToModelState(ValidationResult result, ModelStateDictionary modelState)
    {
        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
            {
                modelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
        }
    }
}
