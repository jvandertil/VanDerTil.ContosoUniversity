using Microsoft.AspNetCore.Mvc;

namespace VanDerTil.ContosoUniversity.Web.Features.Students;

public class ListStudentsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
