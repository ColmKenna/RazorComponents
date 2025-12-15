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

        // Create TabContext for child TabItems to register themselves
        var tabContext = new TabContext();
        context.Items[typeof(TabContext)] = tabContext;

        // Process child content (this allows child TabItems to register)
        var childContent = await output.GetChildContentAsync();
        var content = childContent.GetContent();

        // Check if any tab-item is selected
        if (!content.Contains("checked=\"checked\""))
        {
            // Try to determine which tab should be selected based on registered tabs
            var selectedTabId = tabContext.GetSelectedTabId();

            if (selectedTabId != null)
            {
                // Use context-based selection (preferred method)
                var tabInputPattern = $"<input class=\"tabs-panel-input\" name=\"tabs\" type=\"radio\" id=\"{selectedTabId}\"";
                var tabInputIndex = content.IndexOf(tabInputPattern, StringComparison.Ordinal);
                if (tabInputIndex != -1)
                {
                    var insertIndex = content.IndexOf("/>", tabInputIndex, StringComparison.Ordinal);
                    if (insertIndex != -1)
                    {
                        content = content.Insert(insertIndex, " checked=\"checked\"");
                    }
                }
            }
            else
            {
                // Fallback to old HTML parsing method for backward compatibility
                // (when no TabItems registered, e.g., in tests with raw HTML)
                var firstTabItemIndex = content.IndexOf("<input class=\"tabs-panel-input\"", StringComparison.Ordinal);
                if (firstTabItemIndex != -1)
                {
                    var insertIndex = content.IndexOf("/>", firstTabItemIndex, StringComparison.Ordinal);
                    content = content.Insert(insertIndex, "checked=\"checked\"");
                }
            }
        }

        output.Content.SetHtmlContent(content);
    }
}