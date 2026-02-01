using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

public class AccountsTrendsModel : PageModel
{
    private readonly ILogger<AccountsTrendsModel> _logger;

    public AccountsTrendsModel(ILogger<AccountsTrendsModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        // Page served, data retrieved by client via API
    }
}