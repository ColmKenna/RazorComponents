using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorComponents.SampleModels;

namespace RazorComponents.Pages.SampleUses.EditArrayTagHelper;

public class EmptyPlaceholderExample : PageModel
{
    [BindProperty]
    public List<Address> BasicItems { get; set; } = new();

    [BindProperty]
    public List<Address> TemplateItems { get; set; } = new();

    [BindProperty]
    public List<Address> StyledItems { get; set; } = new();

    public void OnGet()
    {
        // Initialize with empty collections to show placeholders
        BasicItems = new List<Address>();
        TemplateItems = new List<Address>();
        StyledItems = new List<Address>();
    }

    public void OnPostBasicExample()
    {
        // Add a sample address to demonstrate placeholder behavior
        BasicItems = BasicItems ?? new List<Address>();
        BasicItems.Add(new Address
        {
            Street = "123 Main Street",
            City = "Springfield",
        });
    }

    public void OnPostTemplateExample()
    {
        // Add a sample address to demonstrate placeholder disappearing
        TemplateItems = TemplateItems ?? new List<Address>();
        TemplateItems.Add(new Address
        {
            Street = "456 Oak Avenue",
            City = "Shelbyville",
        });
    }

    public void OnPostStyledExample()
    {
        // Add a sample address to demonstrate with custom styling
        StyledItems = StyledItems ?? new List<Address>();
        StyledItems.Add(new Address
        {
            Street = "789 Elm Boulevard",
            City = "Capital City",
        });
    }
}
