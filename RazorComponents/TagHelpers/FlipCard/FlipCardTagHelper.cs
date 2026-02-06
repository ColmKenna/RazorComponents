using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorComponents.TagHelpers.FlipCard;

public enum FlipDirection
{
    Horizontal,
    Vertical
}

[HtmlTargetElement("flip-card")]
public partial class FlipCardTagHelper : TagHelper
{
    [GeneratedRegex(@"^-?\d+(\.\d+)?(px|rem|em|%|vh|vw|vmin|vmax|ch)$", RegexOptions.IgnoreCase)]
    private static partial Regex CssLengthPattern();

    /// <summary>
    /// The direction the card flips. Default is Horizontal (Y-axis rotation).
    /// </summary>
    [HtmlAttributeName("flip-direction")]
    public FlipDirection FlipDirection { get; set; } = FlipDirection.Horizontal;

    /// <summary>
    /// Fixed dimensions of the card.
    /// When null (default), the card auto-fits to its content.
    /// Width and Height must be valid CSS lengths (number + px/rem/em/%/vh/vw/vmin/vmax/ch).
    /// </summary>
    [HtmlAttributeName("size")]
    public FlipCardSize? Size { get; set; }

    /// <summary>
    /// When true, the card height automatically adjusts to the taller face.
    /// Default is true when Size.Height is not specified.
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

        // Read width/height from Size class
        var width = Size?.Width;
        var height = Size?.Height;

        // Determine if auto-height should be applied
        var useAutoHeight = AutoHeight ?? string.IsNullOrEmpty(height);

        // Build CSS classes
        var flipClass = FlipDirection == FlipDirection.Vertical ? "flip-vertical" : "flip-horizontal";
        var autoHeightClass = useAutoHeight ? "auto-height" : "";
        var cardClasses = $"card {flipClass} {autoHeightClass}".Trim();

        // Build CSS custom properties and validate CSS lengths
        var styleBuilder = new StringBuilder();
        if (!string.IsNullOrEmpty(width))
        {
            string w = width;
            if (!CssLengthPattern().IsMatch(w))
                throw new InvalidOperationException(
                    $"Invalid CSS length for width: '{w}'. Must be a number followed by a valid CSS unit (px, rem, em, %, vh, vw, vmin, vmax, ch).");
            styleBuilder.Append("--flip-card-width: ").Append(w).Append("; ");
        }

        if (!string.IsNullOrEmpty(height))
        {
            string h = height;
            if (!CssLengthPattern().IsMatch(h))
                throw new InvalidOperationException(
                    $"Invalid CSS length for height: '{h}'. Must be a number followed by a valid CSS unit (px, rem, em, %, vh, vw, vmin, vmax, ch).");
            styleBuilder.Append("--flip-card-height: ").Append(h).Append("; ");
        }

        // Container classes with flip-card scope
        var containerClasses = string.IsNullOrEmpty(CssClass)
            ? "card-container flip-card"
            : $"card-container flip-card {CssClass}";

        // HTML-encode titles and button text
        var encoder = HtmlEncoder.Default;
        var frontTitle = encoder.Encode(cardContext.FrontTitle ?? "Front");
        var backTitle = encoder.Encode(cardContext.BackTitle ?? "Back");
        var buttonText = encoder.Encode(ButtonText ?? "Flip");

        // Convert IHtmlContent to string for template
        var frontContentHtml = GetHtmlString(cardContext.FrontContent);
        var backContentHtml = GetHtmlString(cardContext.BackContent);

        // Build the output HTML
        output.TagName = "div";
        output.Attributes.SetAttribute("class", containerClasses);
        if (styleBuilder.Length > 0)
        {
            output.Attributes.SetAttribute("style", styleBuilder.ToString().TrimEnd());
        }

        var html = $@"
<div class=""{cardClasses}"">
    <div class=""card-front"" aria-hidden=""false"">
        <div class=""card-front-header"">
            <h2>{frontTitle}</h2>
            <button type=""button"" class=""rotate-button"" data-flip-card-button aria-pressed=""false"">{buttonText}</button>
        </div>
        <div class=""card-front-content"">
            {frontContentHtml}
        </div>
    </div>
    <div class=""card-back"" aria-hidden=""true"">
        <div class=""card-back-header"">
            <h2>{backTitle}</h2>
            <button type=""button"" class=""rotate-button"" data-flip-card-button aria-pressed=""false"">{buttonText}</button>
        </div>
        <div class=""card-back-content"">
            {backContentHtml}
        </div>
    </div>
</div>";

        output.Content.SetHtmlContent(html);
    }

    private static string GetHtmlString(IHtmlContent? content)
    {
        if (content == null) return string.Empty;
        using var writer = new StringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }
}
