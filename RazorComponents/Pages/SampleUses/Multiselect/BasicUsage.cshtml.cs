using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorComponents.Pages.SampleUses.Multiselect;

public class BasicUsageModel : PageModel
{
    public List<string> AvailableScopes { get; set; } = new()
    {
        "openid",
        "profile",
        "email",
        "api1",
        "api2"
    };

    public List<string> SelectedScopes { get; set; } = new()
    {
        "openid",
        "profile"
    };

    public void OnGet()
    {
    }
}
