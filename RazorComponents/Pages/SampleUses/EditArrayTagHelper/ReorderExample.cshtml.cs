using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorComponents.SampleModels;

namespace RazorComponents.Pages.SampleUses.EditArrayTagHelper;

public class ReorderExample : PageModel
{
    [BindProperty]
    public Person Person { get; set; } = new Person
    {
        FirstName = "Pat",
        LastName = "Lee",
        Addresses = new List<Address>
        {
            new Address { Street = "12 Oak Ave", City = "Denver", State = "CO", Code = "80202" },
            new Address { Street = "55 Pine Rd", City = "Portland", State = "OR", Code = "97201" },
            new Address { Street = "90 Maple St", City = "Austin", State = "TX", Code = "73301" }
        }
    };

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        Person.Addresses = Person.Addresses.Where(a => !a.IsDeleted).ToList();
        TempData["Message"] = "Saved with updated order.";
        return RedirectToPage();
    }
}
