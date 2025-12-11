using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorComponents.SampleModels;

namespace RazorComponents.Pages.SampleUses.EditArrayTagHelper;

public class OnDeleteExample : PageModel
{
    [BindProperty]
    public Person Person { get; set; } = new Person
    {
        FirstName = "Jane",
        LastName = "Smith",
        Addresses = new List<Address>
        {
            new Address { Street = "789 Pine Rd", City = "Chicago", State = "IL", Code = "60601" },
            new Address { Street = "321 Elm St", City = "Boston", State = "MA", Code = "02101" },
            new Address { Street = "654 Maple Ave", City = "Seattle", State = "WA", Code = "98101" }
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

        TempData["Message"] = "Person data saved successfully! Deleted addresses have been removed.";
        return RedirectToPage();
    }
}
