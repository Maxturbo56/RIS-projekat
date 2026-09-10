using Microsoft.AspNetCore.Mvc;

namespace AstralNexus.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
        => User.Identity?.IsAuthenticated == true
            ? RedirectToAction("Index", "Dashboard")
            : View();

    public IActionResult Error() => View();
}
