using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace RazorComponents.Pages.SampleUses.Display
{
    public class BasicUsageModel : PageModel
    {
        [Display(Name = "Full Name")]
        public string Name { get; set; } = "John Doe";

        [Display(Name = "Email Address")]
        public string Email { get; set; } = "john.doe@example.com";

        public void OnGet()
        {
        }
    }
}
