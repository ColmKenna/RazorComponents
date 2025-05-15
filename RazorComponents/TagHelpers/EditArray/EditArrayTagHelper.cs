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
    private const string DisplayModeAttributeName = "asp-display-mode";    [HtmlAttributeName(ViewNameAttributeName)]
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

    [ViewContext]
    public required ViewContext ViewContext { get; set; }
    
    [HtmlAttributeName("id")]
    public string Id { get; set; } 

    private readonly IHtmlHelper _htmlHelper;

    public EditArrayTagHelper(IHtmlHelper htmlHelper)
    {
        _htmlHelper = htmlHelper;
    }    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // Reset the TagHelper output
        output.TagName = "div";
        output.Attributes.SetAttribute("class", "edit-array-container");
        
        // Create an ID for the container to use with JavaScript
        string containerId = $"edit-array-{Id}";
        output.Attributes.SetAttribute("id", containerId);
        
        // Setup HtmlHelper to be used in our views
        (_htmlHelper as IViewContextAware)?.Contextualize(ViewContext);
        
        // Create container for rendered items
        var sb = new StringBuilder();
        
        // Get the model expression prefix from ViewContext
        var modelExpressionPrefix = ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix;
        
        // Extract the property name from the ModelExpression if provided
        string collectionName = For?.Name ?? string.Empty;
        
        // Create a wrapper div for the items
        sb.Append($"<div class=\"edit-array-items\" id=\"{containerId}-items\">");
        
        
        // get the type
        
        // Process each item
        int index = 0;
        foreach (var item in Items)
        {
            // Create a unique ID for this item based on its index and collection name
            var fieldName = GetFieldName(modelExpressionPrefix, collectionName, index);
            var itemId = $"{containerId}-item-{index}";
            
            // Create a wrapper for this item
            sb.Append($"<div class=\"edit-array-item\" id=\"{itemId}\">");
            
            // Check if IsDeleted property is present in the model and add a hidden input if not
            var isDeletedProperty = item.GetType().GetProperty("IsDeleted");            
            
            if (isDeletedProperty == null )
            {                
                // Add hidden input for IsDeleted with default value (false)
                sb.Append($"<input type=\"hidden\" name=\"{fieldName}.IsDeleted\" value=\"false\" data-is-deleted-marker />");
            }

            // Store the original prefix
            var originalPrefix = ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix;
            
            // Set the new prefix for this item
            ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = fieldName;
            
            // Create a new ViewDataDictionary with the item as model
            var viewData = new ViewDataDictionary<object>(ViewContext.ViewData)
            {
                Model = item
            };
            
            if (DisplayMode && !string.IsNullOrEmpty(DisplayViewName))
            {
                // Render display view and edit button in display mode
                sb.Append($"<div class=\"display-container\" id=\"{itemId}-display\">");
                
                // Render the display view with the specific model
                var displayViewContent = await _htmlHelper.PartialAsync(DisplayViewName, item, viewData);
                
                // Capture the HTML output
                using (var writer = new StringWriter())
                {
                    displayViewContent.WriteTo(writer, HtmlEncoder.Default);
                    sb.Append(writer.ToString());
                }
                
                // Move the edit button inside the display-container div
                sb.Append($"<button type=\"button\" class=\"btn btn-sm btn-primary edit-item-btn mt-2\" ");
                sb.Append($" onclick=\"toggleEditMode('{itemId}')\">");
                sb.Append("Edit");
                sb.Append("</button>");
                
                sb.Append($"<button type=\"button\" class=\"btn btn-sm btn-danger delete-item-btn mt-2\" ");
                sb.Append($" onclick=\"markForDeletion('{itemId}')\">");
                sb.Append("Delete");
                sb.Append("</button>");
                
                
                sb.Append("</div>");
                
                // Render editor view (initially hidden)
                sb.Append($"<div class=\"edit-container\" id=\"{itemId}-edit\" style=\"display: none;\">");
                
                // Render the editor view with the specific model
                var editorViewContent = await _htmlHelper.PartialAsync(ViewName, item, viewData);
                
                // Capture the HTML output
                using (var writer = new StringWriter())
                {
                    editorViewContent.WriteTo(writer, HtmlEncoder.Default);
                    sb.Append(writer.ToString());
                }
                
                // Add done button
                sb.Append($"<button type=\"button\" class=\"btn btn-sm btn-success done-edit-btn mt-2\" " +
                           $"onclick=\"toggleEditMode('{itemId}')\">");
                sb.Append("Done");
                sb.Append("</button>");
                
                sb.Append("</div>");
            }
            else
            {
                // Render the view with the specific model (editor only)
                var viewContent = await _htmlHelper.PartialAsync(ViewName, item, viewData);
                
                // Capture the HTML output
                using (var writer = new StringWriter())
                {
                    viewContent.WriteTo(writer, HtmlEncoder.Default);
                    sb.Append(writer.ToString());
                }
            }
            
            // Restore original prefix
            ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = originalPrefix;
            
            // Close item wrapper
            sb.Append("</div>");
            
            index++;
        }
        
        // Close the items wrapper
        sb.Append("</div>");
        
        // Add template item if requested
        if (RenderTemplate)
        {
            string templateId = $"{containerId}-template";
            sb.Append($"<template id=\"{templateId}\">");

            // Generate template with a special index
            var templateFieldName = GetFieldName(modelExpressionPrefix, collectionName, "__index__");



        // Store the original prefix
            var originalPrefix = ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix;
            
            // Set the new prefix for the template
            ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = templateFieldName;
            
            // Create a default model instance for the template
            object? templateModel = null;
            var itemType = Items.GetType().GetGenericArguments().FirstOrDefault();
            if (itemType != null)
            {
                templateModel = Activator.CreateInstance(itemType);
            }
            
            // Create a new ViewDataDictionary with the template model
            var viewData = new ViewDataDictionary<object>(ViewContext.ViewData)
            {
                Model = templateModel
            };
            
            // Create a wrapper for the template item
            sb.Append("<div class=\"edit-array-item\">");

            var name = $"{templateFieldName}.IsDeleted";


            if (DisplayMode && !string.IsNullOrEmpty(DisplayViewName))
            {
                // In display mode, we include both the display and edit templates
                // Display template (initially hidden in the template)
                sb.Append("<div class=\"display-container\" style=\"display: none;\">");
                if (templateModel != null)
                {
                    var displayViewContent = await _htmlHelper.PartialAsync(DisplayViewName, templateModel, viewData);
                    using (var writer = new StringWriter())
                    {
                        displayViewContent.WriteTo(writer, HtmlEncoder.Default);
                        sb.Append(writer.ToString());
                    }
                }
                sb.Append("<button type=\"button\" class=\"btn btn-sm btn-primary edit-item-btn mt-2\" " +
                          "onclick=\"toggleEditMode(this.closest('.edit-array-item').id)\">");
                sb.Append("Edit");
                sb.Append("</button>");
                sb.Append("</div>");
                
                sb.Append($"<button type=\"button\" class=\"btn btn-sm btn-danger delete-item-btn mt-2\" ");
                sb.Append($" onclick=\"markForDeletion(this.closest('.edit-array-item').id)\">");
                sb.Append("Delete");
                sb.Append("</button>");   
                
                
                // Edit template
                sb.Append("<div class=\"edit-container\">");
            }
            
            // Render the template view
            if (templateModel != null) 
            {
                var viewContent = await _htmlHelper.PartialAsync(ViewName, templateModel, viewData);
                using (var writer = new StringWriter())
                {
                    viewContent.WriteTo(writer, HtmlEncoder.Default);
                    var templateContent = writer.ToString();
                    sb.Append(templateContent);
                    if (!templateContent.Contains($"name=\"{name}\"", StringComparison.OrdinalIgnoreCase ))
                    {
                        sb.Append($"<input type=\"hidden\" name=\"{name}\" value=\"false\" data-is-deleted-marker />");
                    }
                }
            }
            
            if (DisplayMode && !string.IsNullOrEmpty(DisplayViewName))
            {
                sb.Append("<button type=\"button\" class=\"btn btn-sm btn-success done-edit-btn mt-2\" " +
                          "onclick=\"toggleEditMode(this.closest('.edit-array-item').id)\">");
                sb.Append("Done");
                sb.Append("</button>");
                sb.Append("</div>");
                
                
            }
            
            // Close the template item wrapper
            sb.Append("</div>");
            
            // Restore original prefix
            ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = originalPrefix;
            
            sb.Append("</template>");
            
            // Add button to create new entry if requested
            if (ShowAddButton)
            {
                sb.Append($"<button type=\"button\" class=\"btn btn-primary mt-2\" onclick=\"addNewItem('{containerId}', '{templateId}')\">");
                sb.Append("Add New Item");
                sb.Append("</button>");
            }
        }
        
        // Set the output content
        output.Content.SetHtmlContent(sb.ToString());
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
}
