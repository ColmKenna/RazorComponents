using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorComponents.SampleModels;

namespace RazorComponents.Pages.SampleUses.EditArrayTagHelper;

public class CustomCssExample : PageModel
{
    [BindProperty]
    public Person Person { get; set; } = new Person
    {
        FirstName = "Sarah",
        LastName = "Johnson",
        Addresses = new List<Address>
        {
            new Address { Street = "321 Pine Rd", City = "Portland", State = "OR", Code = "97201" },
            new Address { Street = "654 Maple Dr", City = "Seattle", State = "WA", Code = "98101" }
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

        Person.Addresses = Person.Addresses.Where(a => !a.IsDeleted).ToList();

        TempData["Message"] = "Changes saved with custom styling!";
        return RedirectToPage();
    }
}
