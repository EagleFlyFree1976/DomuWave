using CPQ.Core.ActionFilters;
using Microsoft.AspNetCore.Mvc;

namespace DomuWave.Unified.Controllers;

[NoAccessTokenRequired]
public class HomeController : Controller
{
    public IActionResult Index() => View();
}
