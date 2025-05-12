using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Linq;

namespace RazorComponents.TagHelpers.Tabs;

[HtmlTargetElement("tab")]
public class TabTagHelper : TagHelper
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("class", "tabs");

        var childContent = await output.GetChildContentAsync();
        var content = childContent.GetContent();

        // Check if any tab-item is selected
        if (!content.Contains("checked=\"checked\""))
        {
            // Find the first tab-item and set it as selected
            var firstTabItemIndex = content.IndexOf("<input class=\"tabs-panel-input\"");
            if (firstTabItemIndex != -1)
            {
                var insertIndex = content.IndexOf("/>", firstTabItemIndex);
                content = content.Insert(insertIndex, "checked=\"checked\"");
            }
        }

        output.Content.SetHtmlContent(content);
    }
}