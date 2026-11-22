using Microsoft.AspNetCore.Mvc;
using VetKlinika.Models;

namespace VetKlinika.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        Response.StatusCode = StatusCodes.Status500InternalServerError;
        return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Status(int code)
    {
        if (code == StatusCodes.Status404NotFound)
        {
            Response.StatusCode = code;
            return View("PageNotFound");
        }

        return new StatusCodeResult(Response.StatusCode >= 400 ? Response.StatusCode : StatusCodes.Status404NotFound);
    }
}