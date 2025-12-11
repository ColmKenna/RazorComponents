using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorComponents.SampleModels;

namespace RazorComponents.Pages.SampleUses.EditArrayTagHelper;

public class AdvancedFeaturesExample : PageModel
{
    [BindProperty]
    public Person Person { get; set; } = new Person
    {
        FirstName = "Michael",
        LastName = "Brown",
        Addresses = new List<Address>
        {
            new Address { Street = "789 Oak Ln", City = "Boston", State = "MA", Code = "02101" },
            new Address { Street = "321 Birch Ave", City = "New York", State = "NY", Code = "10001" },
            new Address { Street = "654 Cedar St", City = "Philadelphia", State = "PA", Code = "19101" }
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

        TempData["Message"] = $"Successfully saved {Person.Addresses.Count} address(es)!";
        return RedirectToPage();
    }
}
