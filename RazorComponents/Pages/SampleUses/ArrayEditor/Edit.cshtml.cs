using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorComponents.Pages.SampleUses.ArrayEditor;

public class Edit : PageModel
{
    [BindProperty]
    public SampleArrayEditorViewModel Form { get; set; } = new SampleArrayEditorViewModel();

    public void OnGet()
    {
        // Pre-populate with some dummy data
        Form.Items = new List<string> { "Apple", "Banana", "Cherry" };
        // No deleted items on GET
    }

    public IActionResult OnPost()
    {

        var activeItems = Form.Items;
        var deletedItems = Form.DeletedItems;

        // You now have two separate lists!
        TempData["Message"] = $"Saved {activeItems.Count} active items and {deletedItems.Count} deleted items.";

        return RedirectToPage();
    }
}