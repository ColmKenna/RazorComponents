using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorComponents.Pages.SampleUses.Multiselect;

public class JavaScriptEventsModel : PageModel
{
    public List<string> AvailableScopes { get; set; } = new()
    {
        "openid",
        "profile",
        "email",
        "api1",
        "api2",
        "offline_access"
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
