using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using VanDerTil.ContosoUniversity.Diagnostics;

namespace VanDerTil.ContosoUniversity.Web.Infrastructure.Filters;

public static class ValidationProblemResponse
{
    public static IActionResult From(ModelStateDictionary modelState)
    {
        Guard.NotNull(modelState);

        return new UnprocessableEntityObjectResult(new
        {
            errors = modelState.ToDictionary(
                kv => kv.Key,
                kv => kv.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? [])
        });
    }
}
