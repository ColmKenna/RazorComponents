using System.Text;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorComponents.TagHelpers.FlipCard;

public enum FlipDirection
{
    Horizontal,
    Vertical
}

[HtmlTargetElement("flip-card")]
public class FlipCardTagHelper : TagHelper
{
    /// <summary>
    /// The direction the card flips. Default is Horizontal (Y-axis rotation).
    /// </summary>
    [HtmlAttributeName("flip-direction")]
    public FlipDirection FlipDirection { get; set; } = FlipDirection.Horizontal;

    /// <summary>
    /// Fixed width of the card (e.g., "300px", "20rem"). 
    /// If not specified, defaults to 300px.
    /// </summary>
    [HtmlAttributeName("width")]
    public string? Width { get; set; }

    /// <summary>
    /// Fixed height of the card (e.g., "300px", "20rem").
    /// If not specified, the card will auto-size to fit content.
    /// </summary>
    [HtmlAttributeName("height")]
    public string? Height { get; set; }

    /// <summary>
    /// When true, the card height automatically adjusts to the taller face.
    /// Default is true when height is not specified.
    /// </summary>
    [HtmlAttributeName("auto-height")]
    public bool? AutoHeight { get; set; }

    /// <summary>
    /// Text shown on the flip button. Default is "Flip".
    /// </summary>
    [HtmlAttributeName("button-text")]
    public string ButtonText { get; set; } = "Flip";

    /// <summary>
    /// Additional CSS classes to apply to the card container.
    /// </summary>
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // Create context for child TagHelpers
        var cardContext = new FlipCardContext();
        context.Items[typeof(FlipCardContext)] = cardContext;

        // Process child content (this triggers card-front and card-back TagHelpers)
        await output.GetChildContentAsync();

        // Determine if auto-height should be applied
        var useAutoHeight = AutoHeight ?? string.IsNullOrEmpty(Height);

        // Build CSS classes
        var flipClass = FlipDirection == FlipDirection.Vertical ? "flip-vertical" : "flip-horizontal";
        var autoHeightClass = useAutoHeight ? "auto-height" : "";
        var cardClasses = $"card {flipClass} {autoHeightClass}".Trim();

        // Build inline styles
        var styleBuilder = new StringBuilder();
        if (!string.IsNullOrEmpty(Width))
        {
            styleBuilder.Append($"width: {Width}; ");
        }

        if (!string.IsNullOrEmpty(Height))
        {
            styleBuilder.Append($"height: {Height}; ");
        }

        var inlineStyle = styleBuilder.Length > 0 ? $" style=\"{styleBuilder}\"" : "";

        // Container classes
        var containerClasses = string.IsNullOrEmpty(CssClass)
            ? "card-container"
            : $"card-container {CssClass}";

        // Build the output HTML
        output.TagName = "div";
        output.Attributes.SetAttribute("class", containerClasses);

        var html = $@"
<div class=""{cardClasses}""{inlineStyle}>
    <div class=""card-front"">
        <div class=""card-front-header"">
            <h2>{cardContext.FrontTitle}</h2>
            <button type=""button"" class=""rotate-button"">{ButtonText}</button>
        </div>
        <div class=""card-front-content"">
            {cardContext.FrontContent}
        </div>
    </div>
    <div class=""card-back"">
        <div class=""card-back-header"">
            <h2>{cardContext.BackTitle}</h2>
            <button type=""button"" class=""rotate-button"">{ButtonText}</button>
        </div>
        <div class=""card-back-content"">
            {cardContext.BackContent}
        </div>
    </div>
</div>";

        output.Content.SetHtmlContent(html);
    }
}
