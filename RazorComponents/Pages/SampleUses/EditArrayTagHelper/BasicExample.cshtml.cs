using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorComponents.SampleModels;

namespace RazorComponents.Pages.SampleUses.EditArrayTagHelper;

public class BasicExample : PageModel
{
    [BindProperty]
    public Person Person { get; set; } = new Person
    {
        FirstName = "Jane",
        LastName = "Smith",
        Addresses = new List<Address>
        {
            new Address { Street = "789 Elm St", City = "Chicago", State = "IL", Code = "60601" }
        }
    };

    public void OnGet()
    {
        // Initialize with default data if needed
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Remove items marked for deletion
        Person.Addresses = Person.Addresses.Where(a => !a.IsDeleted).ToList();

        // In a real application, save to database
        TempData["Message"] = "Changes saved successfully!";
        return RedirectToPage();
    }
}
