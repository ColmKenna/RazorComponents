using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorComponents.Pages.SampleUses.Multiselect;

public class CustomStylingModel : PageModel
{
    public List<string> AvailableColors { get; set; } = new()
    {
        "Red",
        "Green",
        "Blue",
        "Yellow",
        "Purple",
        "Orange"
    };

    public List<string> SelectedColors { get; set; } = new()
    {
        "Blue",
        "Green"
    };

    public void OnGet()
    {
    }
}
