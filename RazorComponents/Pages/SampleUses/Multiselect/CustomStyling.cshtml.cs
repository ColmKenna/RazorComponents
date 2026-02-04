using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorComponents.Pages.SampleUses.Multiselect;

public class CustomStylingModel : PageModel
{
    public List<string> AvailablePermissions { get; set; } = new()
    {
        "Read",
        "Write",
        "Delete",
        "Admin",
        "Export"
    };

    public List<string> SelectedPermissions { get; set; } = new()
    {
        "Read",
        "Write"
    };

    public void OnGet()
    {
    }
}
