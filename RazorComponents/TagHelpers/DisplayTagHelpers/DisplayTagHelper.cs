using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorComponents.TagHelpers.DisplayTagHelpers;

/// <summary>
/// A TagHelper that renders a label and display div for a model property.
/// </summary>
[HtmlTargetElement("display", Attributes = ForAttributeName)]
[OutputElementHint("div")]
public class DisplayTagHelper : TagHelper
{
    /// <summary>
    /// The HTML attribute name for the model expression binding.
    /// </summary>
    private const string ForAttributeName = "asp-for";

    /// <summary>
    /// Gets or sets the model expression for the property to display.
    /// This is a required attribute.
    /// </summary>
    [HtmlAttributeName(ForAttributeName)]
    public required ModelExpression For { get; set; }

    /// <summary>
    /// Gets or sets the CSS class to apply to the generated label element.
    /// </summary>
    [HtmlAttributeName("label-class")]
    public string? LabelClass { get; set; }

    /// <summary>
    /// Gets or sets the CSS class to apply to the generated display div element.
    /// </summary>
    [HtmlAttributeName("display-class")]
    public string? DisplayClass { get; set; }
    
    /// <summary>
    /// Gets or sets a suffix to append to the generated id attributes.
    /// </summary>
    [HtmlAttributeName("id-suffix")]
    public string? IdSuffix { get; set; }

    /// <summary>
    /// Gets or sets the view context for the current view rendering operation.
    /// This is a required property set by the framework.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }
    
    /// <summary>
    /// Processes the display tag helper by rendering a label and display div for the specified model property.
    /// </summary>
    /// <param name="context">The TagHelperContext containing tag information.</param>
    /// <param name="output">The TagHelperOutput where rendered content is written.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the required <see cref="For"/> property is not specified (asp-for attribute missing).
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the <see cref="ViewContext"/> property is not set by the framework.
    /// </exception>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ValidateRequiredProperties();
        
        // Get the full expression path including any prefixes
        var fullHtmlFieldName = ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(For.Name);
        var id = GenerateId(fullHtmlFieldName);

        // Create label
        var label = CreateLabel(id);

        // Create display div
        var displayDiv = CreateDisplayDiv(id);

        // Output
        output.TagName = null; // Remove <display> tag
        output.Content.AppendHtml(label);
        output.Content.AppendHtml(displayDiv);
    }

    /// <summary>
    /// Validates that required properties are set before processing.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when either the <see cref="For"/> or <see cref="ViewContext"/> property is null.
    /// </exception>
    private void ValidateRequiredProperties()
    {
        if (For == null)
        {
            throw new InvalidOperationException(
                $"The required {ForAttributeName} attribute must be specified.");
        }
        
        if (ViewContext == null)
        {
            throw new InvalidOperationException(
                $"The '{nameof(ViewContext)}' property must be set by the framework.");
        }
    }

    /// <summary>
    /// Generates a unique id from the field name, converting special characters to underscores.
    /// </summary>
    /// <param name="fieldName">The field name to convert.</param>
    /// <returns>A valid HTML id string.</returns>
    private string GenerateId(string fieldName)
    {
        var id = fieldName.Replace('.', '_').Replace('[', '_').Replace(']', '_');
        if (!string.IsNullOrEmpty(IdSuffix))
        {
            id += IdSuffix;
        }
        return id;
    }

    /// <summary>
    /// Creates a label element for the display property.
    /// </summary>
    /// <param name="id">The id to associate with the label.</param>
    /// <returns>A configured label TagBuilder.</returns>
    private TagBuilder CreateLabel(string id)
    {
        var label = new TagBuilder("label");
        label.Attributes.Add("for", id);
        if (!string.IsNullOrEmpty(LabelClass))
        {
            label.AddCssClass(LabelClass);
        }
        label.InnerHtml.Append(For.Metadata.DisplayName ?? For.Name);
        return label;
    }

    /// <summary>
    /// Creates a div element to display the model property value.
    /// </summary>
    /// <param name="id">The id to apply to the div.</param>
    /// <returns>A configured display div TagBuilder.</returns>
    private TagBuilder CreateDisplayDiv(string id)
    {
        var displayDiv = new TagBuilder("div");
        displayDiv.Attributes.Add("data-id", id);
        displayDiv.Attributes.Add("data-display-for", id);
        if (!string.IsNullOrEmpty(DisplayClass))
        {
            displayDiv.AddCssClass(DisplayClass);
        }
        displayDiv.InnerHtml.Append(For.Model?.ToString() ?? string.Empty);
        return displayDiv;
    }
}
