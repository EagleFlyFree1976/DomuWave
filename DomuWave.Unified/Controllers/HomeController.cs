using Microsoft.AspNetCore.Mvc;

namespace DomuWave.Unified.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
