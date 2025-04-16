namespace RazorComponents.Pages.SampleUses.ArrayEditor;

public class SampleArrayEditorViewModel
{
    public List<string> Items { get; set; } = new List<string>();
    public List<string> DeletedItems { get; set; } = new List<string>();
}
