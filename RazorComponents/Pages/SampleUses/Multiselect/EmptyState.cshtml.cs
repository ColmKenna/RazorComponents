using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorComponents.Pages.SampleUses.Multiselect;

public class EmptyStateModel : PageModel
{
    public List<string> AvailableItems { get; set; } = new(); // Empty list

    public List<string> SelectedItems { get; set; } = new();

    public void OnGet()
    {
    }
}
