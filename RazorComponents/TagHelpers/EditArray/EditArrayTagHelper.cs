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
    
    [HtmlAttributeName(ViewNameAttributeName)]
    public required string ViewName { get; set; }
    
    [HtmlAttributeName(DisplayViewNameAttributeName)]
    public string? DisplayViewName { get; set; }

    [HtmlAttributeName(ItemsAttributeName)]
    public required IEnumerable<object> Items { get; set; }
    
    [HtmlAttributeName(ModelExpAttributeName)]
    public ModelExpression? For { get; set; }

    [HtmlAttributeName(TemplateAttributeName)]
    public bool RenderTemplate { get; set; } = false;
    
    [HtmlAttributeName(AddButtonAttributeName)]
    public bool ShowAddButton { get; set; } = false;
    
    [HtmlAttributeName(DisplayModeAttributeName)]
    public bool DisplayMode { get; set; } = false;

    [HtmlAttributeName(OnUpdateAttributeName)]
    public string? OnUpdate { get; set; }

    [HtmlAttributeName(EnableReorderAttributeName)]
    public bool EnableReordering { get; set; } = false;

    [HtmlAttributeName(ReorderButtonCssClassAttributeName)]
    public string ReorderButtonCssClass { get; set; } = "btn btn-outline-secondary";

    [HtmlAttributeName(MoveUpButtonTextAttributeName)]
    public string MoveUpButtonText { get; set; } = "Move Up";

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

    [HtmlAttributeName(ContainerCssClassAttributeName)]
    public string ContainerCssClass { get; set; } = "edit-array-container";

    [HtmlAttributeName(ItemCssClassAttributeName)]
    public string ItemCssClass { get; set; } = "edit-array-item";

    [HtmlAttributeName(ButtonCssClassAttributeName)]
    public string ButtonCssClass { get; set; } = "btn";

    [HtmlAttributeName(EmptyPlaceholderAttributeName)]
    public string? EmptyPlaceholder { get; set; }

    [ViewContext]
    public required ViewContext ViewContext { get; set; }
    
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
        sb.Append($"<button type=\"button\" class=\"{encodedCssClass} reorder-btn reorder-up-btn\" data-reorder-direction=\"up\" onclick=\"moveItem('{containerId}','{itemId}',-1)\">");
        sb.Append(upText);
        sb.Append("</button>");
        sb.Append($"<button type=\"button\" class=\"{encodedCssClass} reorder-btn reorder-down-btn\" data-reorder-direction=\"down\" onclick=\"moveItem('{containerId}','{itemId}',1)\">");
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
        sb.Append($"<button type=\"button\" class=\"{encodedCssClass} reorder-btn reorder-up-btn\" data-reorder-direction=\"up\" onclick=\"moveItem('{containerId}', this.closest('.edit-array-item').id, -1)\">");
        sb.Append(upText);
        sb.Append("</button>");
        sb.Append($"<button type=\"button\" class=\"{encodedCssClass} reorder-btn reorder-down-btn\" data-reorder-direction=\"down\" onclick=\"moveItem('{containerId}', this.closest('.edit-array-item').id, 1)\">");
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
        sb.Append($"<button type=\"button\" class=\"{GetEncodedButtonCssClass()} {cssModifier}\" ");

        // Build onclick handler
        var targetId = isTemplate ? "this.closest('.edit-array-item').id" : $"'{itemId}'";
        sb.Append($" onclick=\"{primaryAction}({targetId})");

        // Add callback for delete buttons (OnDelete callback)
        if (string.Equals(buttonType, "delete", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(OnDelete))
        {
            var encodedCallback = HtmlEncoder.Default.Encode(OnDelete);
            sb.Append($"; {encodedCallback}({targetId});");
        }

        // Add callback for done buttons (OnUpdate callback)
        if (string.Equals(buttonType, "done", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrEmpty(OnUpdate))
            {
                var encodedCallback = HtmlEncoder.Default.Encode(OnUpdate);
                sb.Append($"; {encodedCallback}({targetId});");
            }
        }

        sb.Append($"\">{buttonText}</button>");

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
