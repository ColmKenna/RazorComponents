using Microsoft.AspNetCore.Mvc;


// ViewComponents/ArrayEditorViewComponent.cs
namespace RazorComponents.Pages.Shared.Components.ArrayEditor;

public class ArrayEditorViewComponent : ViewComponent
{
    public class ArrayEditorModel
    {
        // The current list of item values (strings)
        public List<string> Items { get; set; } = new List<string>();

        // The input name prefix used when rendering fields in the form
        public string Name { get; set; } = "Items";

        // The list of deleted items (strings)
        public List<string> DeletedItems { get; set; } = new List<string>();

        // The input delete name prefix used when rendering fields in the form
        public string DeleteName { get; set; } = "DeletedItems";
        public string Id { get; set; }

        public string CssClass { get; set; } // Existing property
        public string Title { get; set; } // Add this property
    }

    public IViewComponentResult Invoke(string id, IEnumerable<string> items, string cssClass = "", string name = "Items", string deleteName = "DeletedItems", string title = "")
    {
        var model = new ArrayEditorModel
        {
            Id = id,
            Items = items.ToList() ?? new List<string>(),
            Name = name,
            DeleteName = deleteName,
            CssClass = cssClass, // Existing assignment
            Title = title // Pass the title to the model
        };
        return View(model);
    }
}