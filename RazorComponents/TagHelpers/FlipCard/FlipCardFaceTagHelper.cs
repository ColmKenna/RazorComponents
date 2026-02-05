using Microsoft.AspNetCore.Razor.TagHelpers;
namespace RazorComponents.TagHelpers.FlipCard;
[HtmlTargetElement("card-front", ParentTag = "flip-card")]
[HtmlTargetElement("card-back", ParentTag = "flip-card")]
public class FlipCardFaceTagHelper : TagHelper
{
  [HtmlAttributeName("title")]
  public string Title { get; set; } = "";

  public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
  {
    output.SuppressOutput();

    if (!context.Items.TryGetValue(typeof(FlipCardContext), out var item) ||
        item is not FlipCardContext cardContext)
    {
      return;
    }

    var childContent = await output.GetChildContentAsync();
    var content = childContent.GetContent();
        
    // Check which tag was used
    var isFront = output.TagName.Equals("card-front", StringComparison.OrdinalIgnoreCase);
        
    if (isFront)
    {
      cardContext.FrontContent = content;
      cardContext.FrontTitle = string.IsNullOrEmpty(Title) ? "Front" : Title;
    }
    else
    {
      cardContext.BackContent = content;
      cardContext.BackTitle = string.IsNullOrEmpty(Title) ? "Back" : Title;
    }
  }
}