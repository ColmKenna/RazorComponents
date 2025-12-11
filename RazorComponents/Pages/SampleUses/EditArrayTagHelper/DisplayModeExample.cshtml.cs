using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorComponents.SampleModels;

namespace RazorComponents.Pages.SampleUses.EditArrayTagHelper;

public class DisplayModeExample : PageModel
{
    [BindProperty]
    public Person Person { get; set; } = new Person
    {
        FirstName = "John",
        LastName = "Doe",
        Addresses = new List<Address>
        {
            new Address { Street = "123 Main St", City = "Springfield", State = "IL", Code = "62701" },
            new Address { Street = "456 Oak Ave", City = "Lincoln", State = "NE", Code = "68508" }
        }
    };

    public void OnGet()
    {
        // Initialize if needed
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Remove items marked for deletion
        Person.Addresses = Person.Addresses.Where(a => !a.IsDeleted).ToList();

        TempData["Message"] = "Person data saved successfully!";
        return RedirectToPage();
    }
}
