using Microsoft.AspNetCore.Mvc;

namespace parcial.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        ViewBag.Message = "Portal funcionando correctamente!";
        return View();
    }

    public IActionResult Error()
    {
        return Content("Error 500 - Algo salió mal");
    }
}
