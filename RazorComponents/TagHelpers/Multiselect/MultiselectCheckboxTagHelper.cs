using System.Collections;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorComponents.TagHelpers.Multiselect;

/// <summary>
/// A TagHelper that renders a multiselect checkbox grid control with accessible,
/// styled checkbox options derived from model-bound collections.
/// </summary>
/// <remarks>
/// <para>
/// This TagHelper generates a complete multiselect UI component including:
/// <list type="bullet">
/// <item><description>A title label above the checkbox grid</description></item>
/// <item><description>A description below the title</description></item>
/// <item><description>A fieldset containing all checkbox options</description></item>
/// <item><description>Individual checkboxes for each available item with pill-styled labels</description></item>
/// <item><description>Empty state message when no items exist</description></item>
/// </list>
/// </para>
/// <para>
/// All text content is HTML-encoded to prevent XSS attacks.
/// ARIA attributes are included for accessibility compliance.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;multiselect-checkbox 
///     title="Select Scopes"
///     description="Choose which scopes to allow"
///     multiselect-for="Model.SelectedScopes"
///     multiselect-items="Model.AvailableScopes"
///     id-prefix="scope-checkbox"
///     fieldset-id="scopes-fieldset" /&gt;
/// </code>
/// </example>
[HtmlTargetElement("multiselect-checkbox")]
public class MultiselectCheckboxTagHelper : TagHelper
{
    private static readonly HtmlEncoder Encoder = HtmlEncoder.Default;

    #region Content Properties

    /// <summary>
    /// Gets or sets the title/label displayed above the checkbox grid.
    /// </summary>
    /// <value>The title text. Default is empty string (no title rendered).</value>
    [HtmlAttributeName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description text displayed below the title.
    /// </summary>
    /// <value>The description text. Default is empty string (no description rendered).</value>
    [HtmlAttributeName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message displayed when no items are available.
    /// </summary>
    /// <value>The empty state message. Default is "No items available. Contact your administrator."</value>
    [HtmlAttributeName("empty-message")]
    public string EmptyMessage { get; set; } = "No items available. Contact your administrator.";

    #endregion

    #region Model Binding Properties

    /// <summary>
    /// Gets or sets the model expression for the selected items collection.
    /// Also determines the input name attribute for form binding.
    /// </summary>
    /// <value>A ModelExpression pointing to the selected items property, or null.</value>
    [HtmlAttributeName("multiselect-for")]
    public ModelExpression? For { get; set; }

    /// <summary>
    /// Gets or sets the model expression for the available items to choose from.
    /// </summary>
    /// <value>A ModelExpression pointing to the available items collection, or null.</value>
    [HtmlAttributeName("multiselect-items")]
    public ModelExpression? Items { get; set; }

    #endregion

    #region CSS Class Properties

    /// <summary>
    /// Gets or sets the CSS class for the outer container div.
    /// </summary>
    /// <value>The CSS class name. Default is "form-group".</value>
    [HtmlAttributeName("group-class")]
    public string GroupClass { get; set; } = "form-group";

    /// <summary>
    /// Gets or sets the CSS class for the title label.
    /// </summary>
    /// <value>The CSS class name. Default is "form-label".</value>
    [HtmlAttributeName("label-class")]
    public string LabelClass { get; set; } = "form-label";

    /// <summary>
    /// Gets or sets the CSS class for the description div.
    /// </summary>
    /// <value>The CSS class name. Default is "form-text".</value>
    [HtmlAttributeName("description-class")]
    public string DescriptionClass { get; set; } = "form-text";

    /// <summary>
    /// Gets or sets the CSS class to apply to the fieldset element.
    /// </summary>
    /// <value>The CSS class name. Default is "multiselect-fieldset".</value>
    [HtmlAttributeName("fieldset-class")]
    public string FieldsetClass { get; set; } = "multiselect-fieldset";

    /// <summary>
    /// Gets or sets the CSS class for the grid container div.
    /// </summary>
    /// <value>The CSS class name. Default is "multiselect-grid".</value>
    [HtmlAttributeName("grid-class")]
    public string GridClass { get; set; } = "multiselect-grid";

    /// <summary>
    /// Gets or sets the CSS class for each option container div.
    /// </summary>
    /// <value>The CSS class name. Default is "multiselect-option".</value>
    [HtmlAttributeName("option-class")]
    public string OptionClass { get; set; } = "multiselect-option";

    /// <summary>
    /// Gets or sets the CSS class for checkbox input elements.
    /// </summary>
    /// <value>The CSS class name. Default is "multiselect-input".</value>
    [HtmlAttributeName("input-class")]
    public string InputClass { get; set; } = "multiselect-input";

    /// <summary>
    /// Gets or sets the CSS class for checkbox label elements.
    /// </summary>
    /// <value>The CSS class name. Default is "multiselect-label".</value>
    [HtmlAttributeName("checkbox-label-class")]
    public string CheckboxLabelClass { get; set; } = "multiselect-label";

    /// <summary>
    /// Gets or sets the CSS class for the pill span inside label.
    /// </summary>
    /// <value>The CSS class name. Default is "multiselect-pill".</value>
    [HtmlAttributeName("pill-class")]
    public string PillClass { get; set; } = "multiselect-pill";

    #endregion

    #region ID/Prefix Properties

    /// <summary>
    /// Gets or sets the ID for the fieldset element.
    /// </summary>
    /// <value>The fieldset ID. Default is empty string (no id attribute rendered).</value>
    [HtmlAttributeName("fieldset-id")]
    public string FieldsetId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the prefix for generating checkbox IDs.
    /// </summary>
    /// <value>The ID prefix. Default is "checkbox".</value>
    /// <remarks>
    /// Each checkbox will have an ID of "{IdPrefix}-{index}", where index is 0-based.
    /// </remarks>
    [HtmlAttributeName("id-prefix")]
    public string IdPrefix { get; set; } = "checkbox";

    #endregion

    #region Framework Properties

    /// <summary>
    /// Gets or sets the current ViewContext for model binding support.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = null!;

    #endregion

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        // Remove the original tag
        output.TagName = null;

        var sb = new StringBuilder();

        // Get available items and selected items from model expressions
        var availableItems = GetAvailableItems();
        var selectedItems = GetSelectedItems();
        var inputName = GetInputName();

        // Render the complete component structure
        RenderOuterContainer(sb, availableItems, selectedItems, inputName);

        output.Content.SetHtmlContent(sb.ToString());
    }

    #region Private Rendering Methods

    private void RenderOuterContainer(StringBuilder sb, List<object> availableItems, HashSet<string> selectedItems, string inputName)
    {
        sb.Append($"<div class=\"{Encoder.Encode(GroupClass)}\">");

        RenderTitleLabel(sb);
        RenderDescription(sb);
        RenderFieldset(sb, availableItems, selectedItems, inputName);

        sb.Append("</div>");
    }

    private void RenderTitleLabel(StringBuilder sb)
    {
        if (!string.IsNullOrWhiteSpace(Title))
        {
            sb.Append($"<label class=\"{Encoder.Encode(LabelClass)}\">{Encoder.Encode(Title)}</label>");
        }
    }

    private void RenderDescription(StringBuilder sb)
    {
        if (!string.IsNullOrWhiteSpace(Description))
        {
            sb.Append($"<div class=\"{Encoder.Encode(DescriptionClass)}\">{Encoder.Encode(Description)}</div>");
        }
    }

    private void RenderFieldset(StringBuilder sb, List<object> availableItems, HashSet<string> selectedItems, string inputName)
    {
        // Start fieldset with optional id
        if (!string.IsNullOrEmpty(FieldsetId))
        {
            sb.Append($"<fieldset class=\"{Encoder.Encode(FieldsetClass)}\" id=\"{Encoder.Encode(FieldsetId)}\">");
        }
        else
        {
            sb.Append($"<fieldset class=\"{Encoder.Encode(FieldsetClass)}\">");
        }

        // Render content based on whether items exist
        if (availableItems.Count == 0)
        {
            RenderEmptyMessage(sb);
        }
        else
        {
            RenderCheckboxGrid(sb, availableItems, selectedItems, inputName);
        }

        sb.Append("</fieldset>");
    }

    private void RenderEmptyMessage(StringBuilder sb)
    {
        sb.Append($"<p class=\"empty-message\">{Encoder.Encode(EmptyMessage)}</p>");
    }

    private void RenderCheckboxGrid(StringBuilder sb, List<object> availableItems, HashSet<string> selectedItems, string inputName)
    {
        var ariaLabelledBy = !string.IsNullOrEmpty(FieldsetId) 
            ? $"{FieldsetId}-label" 
            : $"{IdPrefix}-label";

        sb.Append($"<div aria-labelledby=\"{Encoder.Encode(ariaLabelledBy)}\" class=\"{Encoder.Encode(GridClass)}\" role=\"group\">");

        for (int i = 0; i < availableItems.Count; i++)
        {
            RenderCheckboxOption(sb, availableItems[i], i, selectedItems, inputName);
        }

        sb.Append("</div>");
    }

    private void RenderCheckboxOption(StringBuilder sb, object item, int index, HashSet<string> selectedItems, string inputName)
    {
        var itemValue = item?.ToString() ?? string.Empty;
        var checkboxId = $"{IdPrefix}-{index}";
        var ariaDescribedBy = $"{checkboxId}-desc";
        var isChecked = selectedItems.Contains(itemValue);

        // Option container
        sb.Append($"<div class=\"{Encoder.Encode(OptionClass)}\">");

        // Checkbox input with attributes in alphabetical order (matching expected output)
        sb.Append($"<input aria-describedby=\"{Encoder.Encode(ariaDescribedBy)}\"");
        if (isChecked)
        {
            sb.Append(" checked=\"checked\"");
        }
        sb.Append($" class=\"{Encoder.Encode(InputClass)}\"");
        sb.Append($" id=\"{Encoder.Encode(checkboxId)}\"");
        sb.Append($" name=\"{Encoder.Encode(inputName)}\"");
        sb.Append(" type=\"checkbox\"");
        sb.Append($" value=\"{Encoder.Encode(itemValue)}\">");

        // Label with pill
        sb.Append($"<label class=\"{Encoder.Encode(CheckboxLabelClass)}\" for=\"{Encoder.Encode(checkboxId)}\">");
        sb.Append($"<span class=\"{Encoder.Encode(PillClass)}\">{Encoder.Encode(itemValue)}</span>");
        sb.Append("</label>");

        // Close option container
        sb.Append("</div>");
    }

    #endregion

    #region Private Helper Methods

    private List<object> GetAvailableItems()
    {
        if (Items?.Model is not IEnumerable enumerable)
        {
            return new List<object>();
        }

        var items = new List<object>();
        foreach (var item in enumerable)
        {
            if (item != null)
            {
                items.Add(item);
            }
        }
        return items;
    }

    private HashSet<string> GetSelectedItems()
    {
        var selected = new HashSet<string>();

        if (For?.Model is not IEnumerable enumerable)
        {
            return selected;
        }

        foreach (var item in enumerable)
        {
            if (item != null)
            {
                selected.Add(item.ToString() ?? string.Empty);
            }
        }

        return selected;
    }

    private string GetInputName()
    {
        return For?.Name ?? IdPrefix;
    }

    #endregion
}
