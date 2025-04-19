using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace RazorComponents.Pages.Shared.Components.Dialog
{
    public class DialogInputModel
    {
        public string? Id { get; set; }
        public string? Type { get; set; }
    }

    public class DialogViewComponent : ViewComponent
    {
        public string DialogId { get; set; }
        public string Title { get; set; }
        public IEnumerable<DialogInputModel> Inputs { get; set; }
        public string ButtonText { get; set; }

        public IViewComponentResult Invoke(string dialogId, string title, IEnumerable<DialogInputModel> inputs, string buttonText = "Open Dialog")
        {
            DialogId = dialogId;
            Title = title;
            Inputs = inputs;
            ButtonText = buttonText;
            return View(this);
        }
    }
}
