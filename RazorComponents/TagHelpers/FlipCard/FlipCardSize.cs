namespace RazorComponents.TagHelpers.FlipCard;

/// <summary>
/// Represents the fixed dimensions of a flip card.
/// When not provided, the card auto-fits to its content.
/// </summary>
public class FlipCardSize
{
    /// <summary>
    /// Fixed width of the card (e.g., "300px", "20rem").
    /// Must be a valid CSS length (number + px/rem/em/%/vh/vw/vmin/vmax/ch).
    /// </summary>
    public string? Width { get; set; }

    /// <summary>
    /// Fixed height of the card (e.g., "300px", "20rem").
    /// Must be a valid CSS length (number + px/rem/em/%/vh/vw/vmin/vmax/ch).
    /// </summary>
    public string? Height { get; set; }
}
