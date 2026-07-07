using Microsoft.AspNetCore.Mvc.Filters;

namespace VanDerTil.ContosoUniversity.Web.Infrastructure.Filters;

/// <summary>
/// An action filter that checks the model state before executing an action. If the model state is invalid, it returns a validation problem response.
/// </summary>
public sealed class ModelStateActionFilter : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context) { }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            context.Result = ValidationProblemResponse.From(context.ModelState);
        }
    }
}
