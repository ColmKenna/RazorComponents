using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorComponents.Pages.Shared.Components.Dialog;
using System.Collections.Generic;

namespace RazorComponents.Pages.SampleUses.Dialog
{
    public class ExampleModel : PageModel
    {
        public string DialogId { get; set; } = "sampleDialog";
        public string Title { get; set; } = "Sample Dialog";
        public string ButtonTitle { get; set; } = "Open Dialog";
        public List<DialogInputModel> Inputs { get; set; } = new List<DialogInputModel>
        {
            new DialogInputModel { Id = "username", Type = "text" },
            new DialogInputModel { Id = "age", Type = "number" }
        };
    }
}
