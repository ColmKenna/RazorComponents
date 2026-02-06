using System.Collections;
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
/// <item><description>A description above the fieldset</description></item>
/// <item><description>A fieldset containing a legend (title) and all checkbox options</description></item>
/// <item><description>Individual checkboxes for each available item with pill-styled labels</description></item>
/// <item><description>Empty state message when no items exist</description></item>
/// </list>
/// </para>
/// <para>
/// All text content is HTML-encoded to prevent XSS attacks.
/// ARIA attributes are included for accessibility compliance.
/// </para>
/// <para>
/// <strong>JavaScript Events:</strong> The companion script <c>multiselect-checkbox.js</c>
/// dispatches two custom events on checkbox interaction:
/// <list type="bullet">
/// <item>
/// <term><c>multiselectChanged</c></term>
/// <description>Fired on the grid container when any checkbox changes.
/// <c>event.detail</c> contains <c>{ selectedValues: string[] }</c>.</description>
/// </item>
/// <item>
/// <term><c>multiselectItemToggled</c></term>
/// <description>Fired on the individual checkbox element when it is toggled.
/// <c>event.detail</c> contains <c>{ value: string, checked: bool, selectedValues: string[] }</c>.</description>
/// </item>
/// </list>
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
    private const string DefaultIdPrefix = "checkbox";

    #region Content Properties

    /// <summary>
    /// Gets or sets the title displayed as a legend inside the fieldset.
    /// </summary>
    /// <value>The title text. Default is empty string (no legend rendered).</value>
    [HtmlAttributeName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description text displayed above the fieldset.
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
    /// Gets or sets the CSS class for the title legend element.
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

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;

        var availableItems = GetAvailableItems();
        var selectedItems = GetSelectedItems();
        var inputName = GetInputName();

        var container = BuildOuterContainer(availableItems, selectedItems, inputName);
        output.Content.SetHtmlContent(container);
    }

    #region Private Rendering Methods

    private TagBuilder BuildOuterContainer(List<object> availableItems, HashSet<string> selectedItems, string inputName)
    {
        var container = new TagBuilder("div");
        container.AddCssClass(GroupClass);

        if (!string.IsNullOrWhiteSpace(Description))
        {
            container.InnerHtml.AppendHtml(BuildDescription());
        }

        container.InnerHtml.AppendHtml(BuildFieldset(availableItems, selectedItems, inputName));

        return container;
    }

    private TagBuilder BuildDescription()
    {
        var desc = new TagBuilder("div");
        desc.AddCssClass(DescriptionClass);
        desc.InnerHtml.Append(Description);
        return desc;
    }

    private TagBuilder BuildFieldset(List<object> availableItems, HashSet<string> selectedItems, string inputName)
    {
        var fieldset = new TagBuilder("fieldset");
        fieldset.AddCssClass(FieldsetClass);

        if (!string.IsNullOrEmpty(FieldsetId))
        {
            fieldset.Attributes["id"] = FieldsetId;
        }

        // Legend (title) inside fieldset
        if (!string.IsNullOrWhiteSpace(Title))
        {
            fieldset.InnerHtml.AppendHtml(BuildLegend());
        }

        if (availableItems.Count == 0)
        {
            fieldset.InnerHtml.AppendHtml(BuildEmptyMessage());
        }
        else
        {
            fieldset.InnerHtml.AppendHtml(BuildCheckboxGrid(availableItems, selectedItems, inputName));
        }

        return fieldset;
    }

    private TagBuilder BuildLegend()
    {
        var legend = new TagBuilder("legend");
        legend.AddCssClass(LabelClass);
        legend.Attributes["id"] = GetLabelId();
        legend.InnerHtml.Append(Title);
        return legend;
    }

    private TagBuilder BuildEmptyMessage()
    {
        var p = new TagBuilder("p");
        p.AddCssClass("empty-message");
        p.InnerHtml.Append(EmptyMessage);
        return p;
    }

    private TagBuilder BuildCheckboxGrid(List<object> availableItems, HashSet<string> selectedItems, string inputName)
    {
        var grid = new TagBuilder("div");
        grid.AddCssClass(GridClass);
        grid.Attributes["data-multiselect-grid"] = "";
        grid.Attributes["role"] = "group";

        // Only add aria-labelledby when a title/legend is actually rendered
        if (!string.IsNullOrWhiteSpace(Title))
        {
            grid.Attributes["aria-labelledby"] = GetLabelId();
        }

        for (int i = 0; i < availableItems.Count; i++)
        {
            grid.InnerHtml.AppendHtml(BuildCheckboxOption(availableItems[i], i, selectedItems, inputName));
        }

        return grid;
    }

    private TagBuilder BuildCheckboxOption(object item, int index, HashSet<string> selectedItems, string inputName)
    {
        var itemValue = item?.ToString() ?? string.Empty;
        var safePrefix = GetSafeIdPrefix();
        var checkboxId = $"{safePrefix}-{index}";
        var isChecked = selectedItems.Contains(itemValue);

        // Option container
        var option = new TagBuilder("div");
        option.AddCssClass(OptionClass);
        option.Attributes["data-multiselect-option"] = "";

        // Checkbox input
        var input = new TagBuilder("input");
        if (isChecked)
        {
            input.Attributes["checked"] = "checked";
        }
        input.AddCssClass(InputClass);
        input.Attributes["id"] = checkboxId;
        input.Attributes["name"] = inputName;
        input.Attributes["type"] = "checkbox";
        input.Attributes["value"] = itemValue;
        input.TagRenderMode = TagRenderMode.SelfClosing;
        option.InnerHtml.AppendHtml(input);

        // Label with pill
        var label = new TagBuilder("label");
        label.AddCssClass(CheckboxLabelClass);
        label.Attributes["for"] = checkboxId;

        var pill = new TagBuilder("span");
        pill.AddCssClass(PillClass);
        pill.InnerHtml.Append(itemValue);
        label.InnerHtml.AppendHtml(pill);

        option.InnerHtml.AppendHtml(label);

        return option;
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
        return For?.Name ?? GetSafeIdPrefix();
    }

    /// <summary>
    /// Gets a safe ID prefix, falling back to default if empty or whitespace.
    /// </summary>
    private string GetSafeIdPrefix()
    {
        return string.IsNullOrWhiteSpace(IdPrefix) ? DefaultIdPrefix : IdPrefix;
    }

    /// <summary>
    /// Gets the label ID for aria-labelledby references.
    /// Uses FieldsetId if available, otherwise uses safe IdPrefix.
    /// </summary>
    private string GetLabelId()
    {
        return !string.IsNullOrEmpty(FieldsetId)
            ? $"{FieldsetId}-label"
            : $"{GetSafeIdPrefix()}-label";
    }

    #endregion
}
