using CPQ.Core.ActionFilters;
using Microsoft.AspNetCore.Mvc;

namespace DomuWave.Unified.Controllers;

[NoAccessTokenRequired]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        var wwwroot = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
            ? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "DomuWave.Web", "wwwroot"))
            : Path.Combine(AppContext.BaseDirectory, "wwwroot");
        var indexPath = Path.Combine(wwwroot, "index.html");
        return PhysicalFile(indexPath, "text/html");
    }
}
