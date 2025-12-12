using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace RazorComponents.Pages.SampleUses.Display
{
    public class CustomStylingModel : PageModel
    {
        [Display(Name = "Status")]
        public string Status { get; set; } = "Active";

        [Display(Name = "Priority")]
        public string Priority { get; set; } = "High";

        public void OnGet()
        {
        }
    }
}
