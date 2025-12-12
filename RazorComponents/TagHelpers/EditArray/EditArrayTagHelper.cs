using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorComponents.TagHelpers.EditArray;

[HtmlTargetElement("edit-array", Attributes = ItemsAttributeName)]
public class EditArrayTagHelper : TagHelper
{      
    private const string ItemsAttributeName = "asp-items";
    private const string ViewNameAttributeName = "asp-view-name";
    private const string DisplayViewNameAttributeName = "asp-display-view-name";
    private const string ModelExpAttributeName = "asp-for";
    private const string TemplateAttributeName = "asp-template";
    private const string AddButtonAttributeName = "asp-add-button";
    private const string DisplayModeAttributeName = "asp-display-mode";    
    private const string OnUpdateAttributeName = "asp-on-update";
    private const string OnDeleteAttributeName = "asp-on-delete";
    private const string ContainerCssClassAttributeName = "asp-container-class";
    private const string ItemCssClassAttributeName = "asp-item-class";
    private const string ButtonCssClassAttributeName = "asp-button-class";
    private const string EmptyPlaceholderAttributeName = "asp-empty-placeholder";
    private const string EnableReorderAttributeName = "asp-enable-reordering";
    private const string ReorderButtonCssClassAttributeName = "asp-reorder-button-class";
    private const string MoveUpButtonTextAttributeName = "asp-move-up-text";
    private const string MoveDownButtonTextAttributeName = "asp-move-down-text";
    
    /// <summary>
    /// Gets or sets the name of the partial view used to render each item in edit mode.
    /// This property is required.
    /// </summary>
    /// <value>
    /// The path to the partial view (e.g., "~/Views/Shared/EditorTemplates/PersonEditor.cshtml" or "_PersonEditor").
    /// </value>
    /// <remarks>
    /// The view specified must exist and be accessible from the current context. The view will receive
    /// the item as its model and can use tag helpers and HTML helpers for form binding.
    /// </remarks>
    [HtmlAttributeName(ViewNameAttributeName)]
    public required string ViewName { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the partial view used to render each item in display mode.
    /// Required when <see cref="DisplayMode"/> is <c>true</c>.
    /// </summary>
    /// <value>
    /// The path to the partial view for display mode, or <c>null</c> if display mode is not used.
    /// </value>
    /// <remarks>
    /// <para>
    /// When <see cref="DisplayMode"/> is enabled, items are initially rendered using this view.
    /// Users can toggle between display and edit modes using Edit/Done buttons.
    /// </para>
    /// <para>
    /// If <see cref="DisplayMode"/> is <c>true</c> and this property is <c>null</c>, empty, or whitespace,
    /// an <see cref="InvalidOperationException"/> will be thrown during validation.
    /// </para>
    /// </remarks>
    [HtmlAttributeName(DisplayViewNameAttributeName)]
    public string? DisplayViewName { get; set; }

    /// <summary>
    /// Gets or sets the collection of items to render in the edit array.
    /// This property is required.
    /// </summary>
    /// <value>
    /// An enumerable collection of objects to render. Use an empty collection if there are no items.
    /// Must not be <c>null</c>.
    /// </value>
    /// <remarks>
    /// Each item in the collection will be rendered using the view specified by <see cref="ViewName"/>.
    /// If <see cref="DisplayMode"/> is enabled, items are initially rendered using <see cref="DisplayViewName"/>.
    /// The collection is enumerated once during rendering.
    /// </remarks>
    [HtmlAttributeName(ItemsAttributeName)]
    public required IEnumerable<object> Items { get; set; }
    
    /// <summary>
    /// Gets or sets the model expression for the collection property being edited.
    /// Optional but recommended for proper model binding.
    /// </summary>
    /// <value>
    /// A <see cref="ModelExpression"/> representing the collection property, or <c>null</c> if not specified.
    /// </value>
    /// <remarks>
    /// <para>
    /// When specified, this expression is used to generate proper field names for model binding (e.g., "Person.Addresses[0].Street").
    /// If not specified, field names will be generated based on the ViewContext's template prefix.
    /// </para>
    /// <para>
    /// This property is particularly important when the edit array is used within a larger form and needs to participate
    /// in model binding for form submissions.
    /// </para>
    /// </remarks>
    [HtmlAttributeName(ModelExpAttributeName)]
    public ModelExpression? For { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to render a template section for adding new items.
    /// </summary>
    /// <value>
    /// <c>true</c> to render a template section; otherwise, <c>false</c>. Default is <c>false</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When enabled, an HTML <c>&lt;template&gt;</c> element is rendered containing a blueprint for new items.
    /// This template uses placeholder field names (e.g., "[__index__]") that must be replaced with actual
    /// indices when items are added via JavaScript.
    /// </para>
    /// <para>
    /// Typically used in conjunction with <see cref="ShowAddButton"/> to allow users to dynamically add new items
    /// to the collection.
    /// </para>
    /// </remarks>
    [HtmlAttributeName(TemplateAttributeName)]
    public bool RenderTemplate { get; set; } = false;
    
    /// <summary>
    /// Gets or sets a value indicating whether to render an "Add New Item" button.
    /// </summary>
    /// <value>
    /// <c>true</c> to show the add button; otherwise, <c>false</c>. Default is <c>false</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When enabled, an "Add New Item" button is rendered that allows users to dynamically add new items
    /// to the collection. This button invokes the JavaScript <c>addNewItem()</c> function.
    /// </para>
    /// <para>
    /// Requires <see cref="RenderTemplate"/> to be <c>true</c> to function properly, as the button clones
    /// the template to create new items.
    /// </para>
    /// </remarks>
    [HtmlAttributeName(AddButtonAttributeName)]
    public bool ShowAddButton { get; set; } = false;
    
    /// <summary>
    /// Gets or sets a value indicating whether items should be rendered in display mode by default.
    /// </summary>
    /// <value>
    /// <c>true</c> to show items in display mode initially; otherwise, <c>false</c>. Default is <c>false</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When enabled, each item is rendered with both a display view and an edit view. The display view is shown
    /// by default, and users can toggle to edit mode using Edit/Done buttons. This provides a better user experience
    /// for read-heavy scenarios.
    /// </para>
    /// <para>
    /// Requires <see cref="DisplayViewName"/> to be specified. If <see cref="DisplayViewName"/> is <c>null</c>, empty,
    /// or whitespace when this property is <c>true</c>, an <see cref="InvalidOperationException"/> will be thrown.
    /// </para>
    /// </remarks>
    [HtmlAttributeName(DisplayModeAttributeName)]
    public bool DisplayMode { get; set; } = false;

    /// <summary>
    /// Gets or sets the JavaScript function name to invoke when a user completes editing an item (clicks "Done").
    /// The function receives the item's DOM element ID as a parameter.
    /// </summary>
    /// <value>
    /// The name of a JavaScript function to call, or <c>null</c> if no callback is needed.
    /// </value>
    /// <remarks>
    /// <para>
    /// When set, "Done" buttons will invoke both the <c>toggleEditMode</c> function and this custom callback.
    /// The callback is executed after the toggle completes. If <c>null</c> or empty, only <c>toggleEditMode</c> is called.
    /// </para>
    /// <para>
    /// The callback value is HTML-encoded to prevent XSS vulnerabilities. Only specify the function name;
    /// do not include quotes, parentheses, or other JavaScript code beyond the function identifier.
    /// </para>
    /// <para>
    /// This is particularly useful for triggering AJAX saves, form validation, or other custom logic when
    /// an item's editing is completed.
    /// </para>
    /// </remarks>
    /// <example>
    /// &lt;edit-array asp-items="Model.Items" asp-view-name="ItemEditor"
    ///              asp-display-mode="true" asp-display-view-name="ItemDisplay"
    ///              asp-on-update="saveItemChanges" /&gt;
    ///
    /// &lt;script&gt;
    ///     function saveItemChanges(itemId) {
    ///         console.log('Item updated: ' + itemId);
    ///         // Perform AJAX save, validation, or other logic
    ///     }
    /// &lt;/script&gt;
    /// </example>
    [HtmlAttributeName(OnUpdateAttributeName)]
    public string? OnUpdate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to enable item reordering functionality.
    /// </summary>
    /// <value>
    /// <c>true</c> to enable reordering with Move Up/Down buttons; otherwise, <c>false</c>. Default is <c>false</c>.
    /// </value>
    /// <remarks>
    /// When enabled, each item will have "Move Up" and "Move Down" buttons that allow users to reorder items
    /// in the collection. The reorder buttons invoke the JavaScript <c>moveItem()</c> function.
    /// The container element will also have a <c>data-reorder-enabled="true"</c> attribute.
    /// </remarks>
    [HtmlAttributeName(EnableReorderAttributeName)]
    public bool EnableReordering { get; set; } = false;

    /// <summary>
    /// Gets or sets the CSS class(es) to apply to reorder buttons (Move Up/Down).
    /// </summary>
    /// <value>
    /// A string containing one or more CSS classes separated by spaces.
    /// Default is "btn btn-outline-secondary".
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is only used when <see cref="EnableReordering"/> is <c>true</c>.
    /// If this property is <c>null</c>, empty, or whitespace, the value of <see cref="ButtonCssClass"/> is used instead.
    /// </para>
    /// <para>
    /// The value is HTML-encoded before output to prevent XSS attacks.
    /// </para>
    /// </remarks>
    [HtmlAttributeName(ReorderButtonCssClassAttributeName)]
    public string ReorderButtonCssClass { get; set; } = "btn btn-outline-secondary";

    /// <summary>
    /// Gets or sets the text displayed on the "Move Up" button.
    /// </summary>
    /// <value>
    /// The button text. Default is "Move Up".
    /// </value>
    /// <remarks>
    /// This property is only used when <see cref="EnableReordering"/> is <c>true</c>.
    /// The value is HTML-encoded before output to prevent XSS attacks.
    /// </remarks>
    [HtmlAttributeName(MoveUpButtonTextAttributeName)]
    public string MoveUpButtonText { get; set; } = "Move Up";

    /// <summary>
    /// Gets or sets the text displayed on the "Move Down" button.
    /// </summary>
    /// <value>
    /// The button text. Default is "Move Down".
    /// </value>
    /// <remarks>
    /// This property is only used when <see cref="EnableReordering"/> is <c>true</c>.
    /// The value is HTML-encoded before output to prevent XSS attacks.
    /// </remarks>
    [HtmlAttributeName(MoveDownButtonTextAttributeName)]
    public string MoveDownButtonText { get; set; } = "Move Down";

    /// <summary>
    /// Gets or sets the JavaScript function name to invoke after an item is marked for deletion.
    /// The function receives the item's DOM element ID as a parameter.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When set, delete buttons will invoke both the markForDeletion function and this custom callback.
    /// The callback is executed after markForDeletion completes. If null or empty, only markForDeletion is called.
    /// </para>
    /// <para>
    /// The callback value is HTML-encoded to prevent XSS vulnerabilities. Only specify the function name;
    /// do not include quotes, parentheses, or other JavaScript code beyond the function identifier.
    /// </para>
    /// </remarks>
    /// <example>
    /// &lt;edit-array asp-items="Model.Items" asp-view-name="ItemEditor" 
    ///              asp-display-mode="true" asp-display-view-name="ItemDisplay"
    ///              asp-on-delete="handleItemDeleted" /&gt;
    /// 
    /// &lt;script&gt;
    ///     function handleItemDeleted(itemId) {
    ///         console.log('Item deleted: ' + itemId);
    ///         // Perform custom logic like AJAX calls, animations, etc.
    ///     }
    /// &lt;/script&gt;
    /// </example>
    [HtmlAttributeName(OnDeleteAttributeName)]
    public string? OnDelete { get; set; }

    /// <summary>
    /// Gets or sets the CSS class(es) to apply to the outer container element.
    /// </summary>
    /// <value>
    /// A string containing one or more CSS classes separated by spaces.
    /// Default is "edit-array-container".
    /// </value>
    /// <remarks>
    /// The value is HTML-encoded before output to prevent XSS attacks.
    /// </remarks>
    [HtmlAttributeName(ContainerCssClassAttributeName)]
    public string ContainerCssClass { get; set; } = "edit-array-container";

    /// <summary>
    /// Gets or sets the CSS class(es) to apply to each item wrapper element.
    /// </summary>
    /// <value>
    /// A string containing one or more CSS classes separated by spaces.
    /// Default is "edit-array-item".
    /// </value>
    /// <remarks>
    /// This class is applied to the div that wraps each individual item in the collection.
    /// The value is HTML-encoded before output to prevent XSS attacks.
    /// </remarks>
    [HtmlAttributeName(ItemCssClassAttributeName)]
    public string ItemCssClass { get; set; } = "edit-array-item";

    /// <summary>
    /// Gets or sets the base CSS class(es) to apply to all buttons (Edit, Delete, Done, Add).
    /// </summary>
    /// <value>
    /// A string containing one or more CSS classes separated by spaces.
    /// Default is "btn".
    /// </value>
    /// <remarks>
    /// <para>
    /// This class is applied to all buttons generated by the tag helper, including Edit, Delete, Done, and Add buttons.
    /// Additional button-specific classes (e.g., "btn-primary", "btn-danger") are appended to this base class.
    /// </para>
    /// <para>
    /// The value is HTML-encoded before output to prevent XSS attacks.
    /// </para>
    /// </remarks>
    [HtmlAttributeName(ButtonCssClassAttributeName)]
    public string ButtonCssClass { get; set; } = "btn";

    /// <summary>
    /// Gets or sets the text to display when the collection is empty.
    /// </summary>
    /// <value>
    /// The placeholder text, or <c>null</c> to display nothing when the collection is empty.
    /// </value>
    /// <remarks>
    /// <para>
    /// When the <see cref="Items"/> collection is empty and this property is set, the specified text is
    /// rendered inside a div with the class "edit-array-placeholder". This provides user feedback that
    /// the list is empty rather than showing a blank area.
    /// </para>
    /// <para>
    /// The value is HTML-encoded before output to prevent XSS attacks.
    /// </para>
    /// </remarks>
    [HtmlAttributeName(EmptyPlaceholderAttributeName)]
    public string? EmptyPlaceholder { get; set; }

    /// <summary>
    /// Gets or sets the view context for rendering partial views.
    /// This property is automatically populated by the ASP.NET Core framework.
    /// </summary>
    /// <value>
    /// The current <see cref="Microsoft.AspNetCore.Mvc.Rendering.ViewContext"/>.
    /// Must not be <c>null</c>.
    /// </value>
    /// <remarks>
    /// This property is marked with the <see cref="ViewContextAttribute"/> and is automatically
    /// injected by the framework. It provides access to the current view's context, including
    /// ViewData, TempData, and the HttpContext. This is used internally for rendering partial views
    /// and maintaining proper model binding prefixes.
    /// </remarks>
    [ViewContext]
    public required ViewContext ViewContext { get; set; }
    
    /// <summary>
    /// Gets or sets the HTML id attribute for the edit array container.
    /// This property is required.
    /// </summary>
    /// <value>
    /// A unique identifier for the container element. Must not be <c>null</c>, empty, or whitespace.
    /// </value>
    /// <remarks>
    /// <para>
    /// The id is used to generate unique identifiers for all child elements and is referenced by
    /// JavaScript functions for add, edit, delete, and reorder operations. Each item's id is derived
    /// from this container id (e.g., if id="myArray", items will have ids like "edit-array-myArray-item-0").
    /// </para>
    /// <para>
    /// The value is HTML-encoded before output to prevent XSS attacks. The encoded id is safe for use
    /// in both HTML attributes and JavaScript string literals.
    /// </para>
    /// <para>
    /// An <see cref="InvalidOperationException"/> is thrown during validation if this property is not set.
    /// </para>
    /// </remarks>
    [HtmlAttributeName("id")]
    public string Id { get; set; } 

    private readonly IHtmlHelper _htmlHelper;

    public EditArrayTagHelper(IHtmlHelper htmlHelper)
    {
        _htmlHelper = htmlHelper ?? throw new ArgumentNullException(nameof(htmlHelper));
    }    

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // Validate required configuration
        ValidateConfiguration();
        
        // Reset the TagHelper output
        output.TagName = "div";
        output.Attributes.SetAttribute("class", GetEncodedContainerCssClass());
        
        // Create an ID for the container to use with JavaScript (with encoding for security)
        string containerId = GetEncodedContainerId();
        output.Attributes.SetAttribute("id", containerId);
        if (EnableReordering)
        {
            output.Attributes.SetAttribute("data-reorder-enabled", "true");
        }
        
        // Setup HtmlHelper to be used in our views
        (_htmlHelper as IViewContextAware)?.Contextualize(ViewContext);
        
        // Create container for rendered items and template sections
        var sb = new StringBuilder(EstimateInitialCapacity());

        // Get the model expression prefix from ViewContext
        var modelExpressionPrefix = ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix;

        // Extract the property name from the ModelExpression if provided
        string collectionName = For?.Name ?? string.Empty;

        // Render items
        await RenderItems(sb, containerId, modelExpressionPrefix, collectionName);

        // Render template section when requested
        if (RenderTemplate)
        {
            await RenderTemplateSection(sb, containerId, modelExpressionPrefix, collectionName);
        }

        // Set the output content
        output.Content.SetHtmlContent(sb.ToString());
    }

    private async Task<bool> RenderItems(StringBuilder sb, string containerId, string modelExpressionPrefix, string collectionName)
    {
        sb.Append("<div class=\"edit-array-items\" id=\"")
          .Append(containerId)
          .Append("-items\">");

        var hasItems = false;
        var index = 0;

        foreach (var item in Items)
        {
            hasItems = true;

            var fieldName = GetFieldName(modelExpressionPrefix, collectionName, index);
            var itemId = $"{containerId}-item-{index}";

            sb.Append("<div class=\"")
              .Append(GetEncodedItemCssClass())
              .Append("\" id=\"")
              .Append(itemId)
              .Append("\">");

            var isDeletedProperty = item.GetType().GetProperty("IsDeleted");

            if (isDeletedProperty == null)
            {
                sb.Append("<input type=\"hidden\" name=\"")
                  .Append(fieldName)
                  .Append(".IsDeleted\" value=\"false\" data-is-deleted-marker />");
            }

            var originalPrefix = ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix;
            ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = fieldName;

            var viewData = new ViewDataDictionary<object>(ViewContext.ViewData)
            {
                Model = item
            };

            if (DisplayMode && !string.IsNullOrEmpty(DisplayViewName))
            {
                await RenderItemDisplayMode(sb, item, itemId, viewData);
            }
            else
            {
                await RenderItemEditMode(sb, item, viewData);
            }

            AppendReorderButtons(sb, containerId, itemId);

            ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = originalPrefix;

            sb.Append("</div>");

            index++;
        }

        if (!hasItems)
        {
            RenderEmptyPlaceholder(sb);
        }

        sb.Append("</div>");

        return hasItems;
    }

    private async Task RenderItemDisplayMode(StringBuilder sb, object item, string itemId, ViewDataDictionary<object> viewData)
    {
        sb.Append("<div class=\"display-container\" id=\"")
          .Append(itemId)
          .Append("-display\">");

        var displayViewContent = await _htmlHelper.PartialAsync(DisplayViewName!, item, viewData);
        using (var writer = new StringWriter())
        {
            displayViewContent.WriteTo(writer, HtmlEncoder.Default);
            sb.Append(writer.ToString());
        }

        sb.Append(GenerateButton("edit", itemId, false));
        sb.Append(GenerateButton("delete", itemId, false));
        sb.Append("</div>");

        sb.Append("<div class=\"edit-container\" id=\"")
          .Append(itemId)
          .Append("-edit\" style=\"display: none;\">");

        var editorViewContent = await _htmlHelper.PartialAsync(ViewName, item, viewData);
        using (var writer = new StringWriter())
        {
            editorViewContent.WriteTo(writer, HtmlEncoder.Default);
            sb.Append(writer.ToString());
        }

        sb.Append(GenerateButton("done", itemId, false));
        sb.Append("</div>");
    }

    private async Task RenderItemEditMode(StringBuilder sb, object item, ViewDataDictionary<object> viewData)
    {
        var viewContent = await _htmlHelper.PartialAsync(ViewName, item, viewData);
        using (var writer = new StringWriter())
        {
            viewContent.WriteTo(writer, HtmlEncoder.Default);
            sb.Append(writer.ToString());
        }
    }

    private async Task RenderTemplateSection(StringBuilder sb, string containerId, string modelExpressionPrefix, string collectionName)
    {
        var templateId = $"{containerId}-template";
        sb.Append("<template id=\"")
          .Append(templateId)
          .Append("\">");

        var templateFieldName = GetFieldName(modelExpressionPrefix, collectionName, "__index__");
        var originalPrefix = ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix;
        ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = templateFieldName;

        object? templateModel = null;
        var itemType = Items.GetType().GetGenericArguments().FirstOrDefault();
        if (itemType != null)
        {
            templateModel = Activator.CreateInstance(itemType);
        }

        var viewData = new ViewDataDictionary<object>(ViewContext.ViewData)
        {
            Model = templateModel
        };

        sb.Append("<div class=\"")
          .Append(GetEncodedItemCssClass())
          .Append("\">");

        var name = $"{templateFieldName}.IsDeleted";

        if (DisplayMode && !string.IsNullOrEmpty(DisplayViewName))
        {
            sb.Append("<div class=\"display-container\" style=\"display: none;\">");
            if (templateModel != null)
            {
                var displayViewContent = await _htmlHelper.PartialAsync(DisplayViewName!, templateModel, viewData);
                using (var writer = new StringWriter())
                {
                    displayViewContent.WriteTo(writer, HtmlEncoder.Default);
                    sb.Append(writer.ToString());
                }
            }

            sb.Append(GenerateButton("edit", null, true));
            sb.Append(GenerateButton("delete", null, true));
            sb.Append("</div>");

            sb.Append("<div class=\"edit-container\">");
        }

        if (templateModel != null)
        {
            var viewContent = await _htmlHelper.PartialAsync(ViewName, templateModel, viewData);
            using (var writer = new StringWriter())
            {
                viewContent.WriteTo(writer, HtmlEncoder.Default);
                var templateContent = writer.ToString();
                sb.Append(templateContent);
                if (!templateContent.Contains($"name=\"{name}\"", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append("<input type=\"hidden\" name=\"")
                      .Append(name)
                      .Append("\" value=\"false\" data-is-deleted-marker />");
                }
            }
        }

        if (DisplayMode && !string.IsNullOrEmpty(DisplayViewName))
        {
            sb.Append(GenerateButton("done", null, true));
            sb.Append("</div>");
        }

        AppendTemplateReorderButtons(sb, containerId);

        sb.Append("</div>");
        ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = originalPrefix;
        sb.Append("</template>");

        if (ShowAddButton)
        {
            sb.Append("<button type=\"button\" class=\"")
              .Append(GetEncodedButtonCssClass())
              .Append(" btn-primary mt-2\" id=\"")
              .Append(containerId)
              .Append("-add\" onclick=\"addNewItem('")
              .Append(containerId)
              .Append("', '")
              .Append(templateId)
              .Append("')\">Add New Item</button>");
        }
    }

    private void RenderEmptyPlaceholder(StringBuilder sb)
    {
        if (string.IsNullOrWhiteSpace(EmptyPlaceholder))
        {
            return;
        }

        sb.Append("<div class=\"edit-array-placeholder\">")
          .Append(HtmlEncoder.Default.Encode(EmptyPlaceholder))
          .Append("</div>");
    }

    private void AppendReorderButtons(StringBuilder sb, string containerId, string itemId)
    {
        if (!EnableReordering)
        {
            return;
        }

        var upText = EncodeButtonText(MoveUpButtonText, "Move Up");
        var downText = EncodeButtonText(MoveDownButtonText, "Move Down");
        var encodedCssClass = GetEncodedReorderButtonCssClass();

        sb.Append("<div class=\"reorder-controls\">");
        sb.Append("<button type=\"button\" class=\"")
            .Append(encodedCssClass)
            .Append(" reorder-btn reorder-up-btn\" data-reorder-direction=\"up\" onclick=\"moveItem('")
            .Append(containerId)
            .Append("','")
            .Append(itemId)
            .Append("',-1)\">");
        sb.Append(upText);
        sb.Append("</button>");
        sb.Append("<button type=\"button\" class=\"")
            .Append(encodedCssClass)
            .Append(" reorder-btn reorder-down-btn\" data-reorder-direction=\"down\" onclick=\"moveItem('")
            .Append(containerId)
            .Append("','")
            .Append(itemId)
            .Append("',1)\">");
        sb.Append(downText);
        sb.Append("</button>");
        sb.Append("</div>");
    }

    private void AppendTemplateReorderButtons(StringBuilder sb, string containerId)
    {
        if (!EnableReordering)
        {
            return;
        }

        var upText = EncodeButtonText(MoveUpButtonText, "Move Up");
        var downText = EncodeButtonText(MoveDownButtonText, "Move Down");
        var encodedCssClass = GetEncodedReorderButtonCssClass();

        sb.Append("<div class=\"reorder-controls\">");
        sb.Append("<button type=\"button\" class=\"")
            .Append(encodedCssClass)
            .Append(" reorder-btn reorder-up-btn\" data-reorder-direction=\"up\" onclick=\"moveItem('")
            .Append(containerId)
            .Append("', this.closest('.edit-array-item').id, -1)\">");
        sb.Append(upText);
        sb.Append("</button>");
        sb.Append("<button type=\"button\" class=\"")
            .Append(encodedCssClass)
            .Append(" reorder-btn reorder-down-btn\" data-reorder-direction=\"down\" onclick=\"moveItem('")
            .Append(containerId)
            .Append("', this.closest('.edit-array-item').id, 1)\">");
        sb.Append(downText);
        sb.Append("</button>");
        sb.Append("</div>");
    }

    private int EstimateInitialCapacity()
    {
        const int baseCapacity = 200;
        const int perItemEstimate = 500;

        var count = 0;
        if (Items != null && Items.TryGetNonEnumeratedCount(out var knownCount))
        {
            count = knownCount;
        }
        else
        {
            count = 10; // fallback guess when count is not cheaply available
        }

        var capacity = baseCapacity + (perItemEstimate * count);
        if (RenderTemplate)
        {
            capacity += perItemEstimate; // extra budget for template markup
        }

        return capacity;
    }

    private string GetReorderButtonCssClass()
    {
        return string.IsNullOrWhiteSpace(ReorderButtonCssClass) ? ButtonCssClass : ReorderButtonCssClass;
    }

    /// <summary>
    /// Gets the container CSS class, properly HTML-encoded for safe output in attributes.
    /// </summary>
    /// <returns>The encoded container CSS class.</returns>
    private string GetEncodedContainerCssClass()
    {
        return HtmlEncoder.Default.Encode(ContainerCssClass);
    }

    /// <summary>
    /// Gets the item CSS class, properly HTML-encoded for safe output in attributes.
    /// </summary>
    /// <returns>The encoded item CSS class.</returns>
    private string GetEncodedItemCssClass()
    {
        return HtmlEncoder.Default.Encode(ItemCssClass);
    }

    /// <summary>
    /// Gets the button CSS class, properly HTML-encoded for safe output in attributes.
    /// </summary>
    /// <returns>The encoded button CSS class.</returns>
    private string GetEncodedButtonCssClass()
    {
        return HtmlEncoder.Default.Encode(ButtonCssClass);
    }

    /// <summary>
    /// Gets the reorder button CSS class, properly HTML-encoded for safe output in attributes.
    /// </summary>
    /// <returns>The encoded reorder button CSS class (falls back to ButtonCssClass if ReorderButtonCssClass is empty).</returns>
    private string GetEncodedReorderButtonCssClass()
    {
        var cssClass = GetReorderButtonCssClass();
        return HtmlEncoder.Default.Encode(cssClass);
    }

    private string EncodeButtonText(string text, string fallback)
    {
        return HtmlEncoder.Default.Encode(string.IsNullOrWhiteSpace(text) ? fallback : text);
    }
    
    private string GetFieldName(string? prefix, string collectionName, object index)
    {
        if (string.IsNullOrEmpty(collectionName))
        {
            return string.IsNullOrEmpty(prefix) 
                ? $"[{index}]" 
                : $"{prefix}[{index}]";
        }
        
        return string.IsNullOrEmpty(prefix)
            ? $"{collectionName}[{index}]"
            : $"{prefix}.{collectionName}[{index}]";
    }
    
    /// <summary>
    /// Generates an HTML button element with appropriate attributes and click handlers.
    /// </summary>
    /// <param name="buttonType">The type of button: "edit", "delete", or "done"</param>
    /// <param name="itemId">The ID of the item (or null for template buttons)</param>
    /// <param name="isTemplate">True if generating button for template, false for item</param>
    /// <returns>The generated button HTML string</returns>
    private string GenerateButton(string buttonType, string? itemId, bool isTemplate = false)
    {
        var sb = new StringBuilder();

        // Determine button-specific properties
        string cssModifier, buttonText, primaryAction;
        switch (buttonType.ToLowerInvariant())
        {
            case "edit":
                cssModifier = "btn-sm btn-primary edit-item-btn mt-2";
                buttonText = "Edit";
                primaryAction = "toggleEditMode";
                break;
            case "delete":
                cssModifier = "btn-sm btn-danger delete-item-btn mt-2";
                buttonText = "Delete";
                primaryAction = "markForDeletion";
                break;
            case "done":
                cssModifier = "btn-sm btn-success done-edit-btn mt-2";
                buttonText = "Done";
                primaryAction = "toggleEditMode";
                break;
            default:
                throw new ArgumentException($"Unknown button type: {buttonType}", nameof(buttonType));
        }

        // Build the button HTML
        sb.Append("<button type=\"button\" class=\"")
          .Append(GetEncodedButtonCssClass())
          .Append(' ')
          .Append(cssModifier)
          .Append("\" ");

        // Build onclick handler
        var targetId = isTemplate ? "this.closest('.edit-array-item').id" : $"'{itemId}'";
        sb.Append(" onclick=\"")
          .Append(primaryAction)
          .Append('(')
          .Append(targetId)
          .Append(')');

        // Add callback for delete buttons (OnDelete callback)
        if (string.Equals(buttonType, "delete", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(OnDelete))
        {
            var encodedCallback = HtmlEncoder.Default.Encode(OnDelete);
            sb.Append("; ")
              .Append(encodedCallback)
              .Append('(')
              .Append(targetId)
              .Append(");");
        }

        // Add callback for done buttons (OnUpdate callback)
        if (string.Equals(buttonType, "done", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrEmpty(OnUpdate))
            {
                var encodedCallback = HtmlEncoder.Default.Encode(OnUpdate);
                sb.Append("; ")
                  .Append(encodedCallback)
                  .Append('(')
                  .Append(targetId)
                  .Append(");");
            }
        }

        sb.Append("\">")
          .Append(buttonText)
          .Append("</button>");

        return sb.ToString();
    }

    /// <summary>
    /// Gets the container ID for use in HTML attributes, properly HTML-encoded for safe output.
    /// </summary>
    /// <remarks>
    /// The user-provided Id is HTML-encoded to prevent XSS attacks. The resulting encoded container ID
    /// is safe for use in both HTML attributes and JavaScript string literals within those attributes
    /// (e.g., onclick="moveItem('edit-array-id')"). All itemIds derived from this containerId inherit
    /// the same safety guarantees.
    /// </remarks>
    /// <returns>The encoded container ID (e.g., "edit-array-myId").</returns>
    private string GetEncodedContainerId()
    {
        // Encode the user-provided Id for safe HTML attribute output
        var encodedId = HtmlEncoder.Default.Encode(Id);
        return $"edit-array-{encodedId}";
    }

    /// <summary>
    /// Validates that all required configuration properties are properly set.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when any required property is not properly configured.</exception>
    private void ValidateConfiguration()
    {
        // Validate ViewName
        if (string.IsNullOrWhiteSpace(ViewName))
        {
            throw new InvalidOperationException(
                $"The '{nameof(ViewName)}' property is required and must not be null or empty. " +
                "Please specify the name of the partial view to render for each item.");
        }
        
        // Validate Items
        if (Items == null)
        {
            throw new InvalidOperationException(
                $"The '{nameof(Items)}' property is required and must not be null. " +
                "Use an empty collection if there are no items to render.");
        }

        // Validate Id (required for JavaScript functionality)
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new InvalidOperationException(
                $"The 'id' attribute is required and must not be null, empty, or whitespace. " +
                "The id is used to generate unique JavaScript function calls and DOM element identifiers.");
        }

        // Validate DisplayMode requires DisplayViewName
        if (DisplayMode && string.IsNullOrWhiteSpace(DisplayViewName))
        {
            throw new InvalidOperationException(
                $"The '{nameof(DisplayViewName)}' property is required when '{nameof(DisplayMode)}' is enabled. " +
                $"Please specify a partial view name for display mode, or set '{nameof(DisplayMode)}' to false.");
        }

        // Validate ViewContext and nested properties
        if (ViewContext == null)
        {
            throw new InvalidOperationException(
                $"The '{nameof(ViewContext)}' property is required and must not be null.");
        }
        
        if (ViewContext.ViewData == null)
        {
            throw new InvalidOperationException(
                "ViewContext.ViewData must not be null. Ensure ViewContext is properly initialized.");
        }
    }
}
