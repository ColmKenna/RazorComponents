using Microsoft.AspNetCore.Html;

namespace RazorComponents.TagHelpers.FlipCard;

public class FlipCardContext
{
    /// <summary>
    /// The rendered HTML content for the front face of the card.
    /// </summary>
    public IHtmlContent? FrontContent { get; set; }

    /// <summary>
    /// The rendered HTML content for the back face of the card.
    /// </summary>
    public IHtmlContent? BackContent { get; set; }

    /// <summary>
    /// Title displayed in the front card header.
    /// </summary>
    public string FrontTitle { get; set; } = "Front";

    /// <summary>
    /// Title displayed in the back card header.
    /// </summary>
    public string BackTitle { get; set; } = "Back";
}
