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

        // Check which tag was used. Output.TagName can be null after SuppressOutput().
        var tagName = context.TagName ?? output.TagName;
        var isFront = string.Equals(tagName, "card-front", StringComparison.OrdinalIgnoreCase);

        if (isFront)
        {
            cardContext.FrontContent = childContent;
            cardContext.FrontTitle = string.IsNullOrEmpty(Title) ? "Front" : Title;
        }
        else
        {
            cardContext.BackContent = childContent;
            cardContext.BackTitle = string.IsNullOrEmpty(Title) ? "Back" : Title;
        }
    }
}
