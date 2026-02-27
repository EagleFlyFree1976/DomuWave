using DomuWave.Services.Models;

namespace DomuWave.Services.Extensions;

public static class MenuItemExtensions
{
    public static bool MatchesVisibility(this MenuItem item, long? tenantId)
    {
        return item.Tags switch
        {
            "TenantRequired" => tenantId != null,
            "NoTenant" => tenantId == null,
            _ => true   // null o "Always" → sempre visibile
        };
    }
}