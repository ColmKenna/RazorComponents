using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorComponents.SampleModels;

namespace RazorComponents.Pages.SampleUses.Display
{
    public class ComplexUsageModel : PageModel
    {
        public Person User { get; set; } = new Person
        {
            FirstName = "Jane",
            LastName = "Smith",
            Addresses = new List<Address>
            {
                new Address { Street = "123 Maple St", City = "Springfield", State = "IL" }
            }
        };

        public void OnGet()
        {
        }
    }
}
