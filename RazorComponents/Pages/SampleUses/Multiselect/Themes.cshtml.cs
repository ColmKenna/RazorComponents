using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorComponents.Pages.SampleUses.Multiselect;

public class ThemesModel : PageModel
{
    public List<string> AvailableFruits { get; set; } = new()
    {
        "Apple",
        "Banana",
        "Cherry",
        "Grape",
        "Mango",
        "Orange"
    };

    public List<string> SelectedFruits { get; set; } = new()
    {
        "Apple",
        "Mango"
    };

    public void OnGet()
    {
    }
}
