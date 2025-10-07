using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorComponents.TagHelpers.DisplayTagHelpers;

[HtmlTargetElement("display", Attributes = "asp-for")]
public class DisplayTagHelper : TagHelper
{
    [HtmlAttributeName("asp-for")]
    public ModelExpression For { get; set; }

    [HtmlAttributeName("label-class")]
    public string? LabelClass { get; set; }

    [HtmlAttributeName("display-class")]
    public string? DisplayClass { get; set; }
    
    [HtmlAttributeName("id-suffix")]
    public string? IdSuffix { get; set; }

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; }
    
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        // Get the full expression path including any prefixes
        var fullHtmlFieldName = ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(For.Name);
        var id = fullHtmlFieldName.Replace('.', '_').Replace('[', '_').Replace(']', '_') + (string.IsNullOrEmpty(IdSuffix) ? "" : IdSuffix);
        var name = fullHtmlFieldName;

        // Create label
        var label = new TagBuilder("label");
        label.Attributes.Add("for", id);
        if (!string.IsNullOrEmpty(LabelClass))
            label.AddCssClass(LabelClass);
        label.InnerHtml.Append(For.Metadata.DisplayName ?? For.Name);

        // Create span
        var span = new TagBuilder("div");
        span.Attributes.Add("data-id", id);
        //display-for
        span.Attributes.Add("data-display-for", id);
 //       span.Attributes.Add("name", name);
        if (!string.IsNullOrEmpty(DisplayClass))
            span.AddCssClass(DisplayClass);
        span.InnerHtml.Append(For.Model?.ToString() ?? string.Empty);

        // Output
        output.TagName = null; // Remove <display> tag
        output.Content.AppendHtml(label);
        output.Content.AppendHtml(span);
    }
}