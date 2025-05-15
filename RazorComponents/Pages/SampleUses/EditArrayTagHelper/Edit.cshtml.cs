using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorComponents.SampleModels;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace RazorComponents.Pages.SampleUses.EditArrayTagHelper;

public class Edit : PageModel
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
        // Nothing to do on GET
    }
    
    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        // In a real application, you would save data to database here
        
        // Redirect to the same page to show the updated data
        return RedirectToPage();
    }
}
