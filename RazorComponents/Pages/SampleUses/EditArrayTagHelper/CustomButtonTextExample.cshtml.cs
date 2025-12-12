using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorComponents.SampleModels;

namespace RazorComponents.Pages.SampleUses.EditArrayTagHelper;

public class CustomButtonTextExample : PageModel
{
    [BindProperty]
    public Person Person { get; set; } = new Person
    {
        FirstName = "Alex",
        LastName = "Thompson",
        Addresses = new List<Address>
        {
            new Address { Street = "789 Oak Avenue", City = "Austin", State = "TX", Code = "78701" },
            new Address { Street = "456 Elm Street", City = "Dallas", State = "TX", Code = "75201" }
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

        TempData["Message"] = "Changes saved with custom button text!";
        return RedirectToPage();
    }
}
