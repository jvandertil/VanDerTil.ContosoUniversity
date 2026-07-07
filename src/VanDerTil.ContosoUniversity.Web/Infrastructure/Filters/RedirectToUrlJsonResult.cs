using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VanDerTil.ContosoUniversity.Web.Infrastructure.Filters;

public static class RedirectToUrlJsonResult
{
    public static IActionResult To(string? url)
    {
        return new JsonResult(new { redirectToUrl = url })
        {
            StatusCode = StatusCodes.Status200OK
        };
    }
}
