using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;

namespace RazorComponents.Tests;

/// <summary>
/// Base class for tag helper tests providing common test helper methods.
/// </summary>
/// <typeparam name="TTagHelper">The tag helper type being tested</typeparam>
public abstract class TagHelperTestBase<TTagHelper> where TTagHelper : TagHelper
{
    #region Tag Helper Creation

    /// <summary>
    /// Creates a tag helper instance with optional configuration action.
    /// NOTE: This method must be overridden in derived classes if the tag helper
    /// has required properties or doesn't have a parameterless constructor.
    /// </summary>
    /// <param name="configure">Optional action to configure the tag helper instance</param>
    /// <returns>Configured tag helper instance</returns>
    protected abstract TTagHelper CreateTagHelper(Action<TTagHelper>? configure = null);

    #endregion

    #region ViewContext Creation

    /// <summary>
    /// Creates a ViewContext for testing with optional HTML field prefix.
    /// </summary>
    /// <param name="htmlFieldPrefix">Optional HTML field prefix for nested models</param>
    /// <returns>Configured ViewContext instance</returns>
    protected static ViewContext CreateViewContext(string? htmlFieldPrefix = null)
    {
        var actionContext = new Microsoft.AspNetCore.Mvc.ActionContext(
            new Microsoft.AspNetCore.Http.DefaultHttpContext(),
            new Microsoft.AspNetCore.Routing.RouteData(),
            new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());

        var viewContext = new ViewContext(
            actionContext,
            Mock.Of<Microsoft.AspNetCore.Mvc.ViewEngines.IView>(),
            new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()),
            Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionary>(),
            TextWriter.Null,
            new HtmlHelperOptions());

        if (htmlFieldPrefix != null)
        {
            viewContext.ViewData.TemplateInfo.HtmlFieldPrefix = htmlFieldPrefix;
        }

        return viewContext;
    }

    #endregion

    #region TagHelperContext Creation

    /// <summary>
    /// Creates a TagHelperContext for testing.
    /// </summary>
    /// <param name="tagName">The tag name (default: "test")</param>
    /// <param name="attributes">Optional tag attributes</param>
    /// <param name="uniqueId">Optional unique ID (default: "test")</param>
    /// <returns>Configured TagHelperContext instance</returns>
    protected static TagHelperContext CreateContext(
        string tagName = "test",
        TagHelperAttributeList? attributes = null,
        string uniqueId = "test")
    {
        return new TagHelperContext(
            tagName: tagName,
            allAttributes: attributes ?? new TagHelperAttributeList(),
            items: new Dictionary<object, object>(),
            uniqueId: uniqueId);
    }

    #endregion

    #region TagHelperOutput Creation

    /// <summary>
    /// Creates a TagHelperOutput for testing.
    /// </summary>
    /// <param name="tagName">The tag name (default: "test")</param>
    /// <param name="tagMode">The tag mode (default: StartTagAndEndTag)</param>
    /// <param name="attributes">Optional tag attributes</param>
    /// <returns>Configured TagHelperOutput instance</returns>
    protected static TagHelperOutput CreateOutput(
        string tagName = "test",
        TagMode tagMode = TagMode.StartTagAndEndTag,
        TagHelperAttributeList? attributes = null)
    {
        return new TagHelperOutput(
            tagName: tagName,
            attributes: attributes ?? new TagHelperAttributeList(),
            getChildContentAsync: (useCached, encoder) =>
                Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()))
        {
            TagMode = tagMode
        };
    }

    /// <summary>
    /// Creates a TagHelperOutput with pre-set HTML content.
    /// </summary>
    /// <param name="childContent">The HTML content to set</param>
    /// <param name="tagName">The tag name (default: "test")</param>
    /// <param name="tagMode">The tag mode (default: StartTagAndEndTag)</param>
    /// <returns>Configured TagHelperOutput instance with content</returns>
    protected static TagHelperOutput CreateOutputWithContent(
        string childContent,
        string tagName = "test",
        TagMode tagMode = TagMode.StartTagAndEndTag)
    {
        var output = new TagHelperOutput(
            tagName: tagName,
            attributes: new TagHelperAttributeList(),
            getChildContentAsync: (useCached, encoder) =>
            {
                var content = new DefaultTagHelperContent();
                content.SetHtmlContent(childContent);
                return Task.FromResult<TagHelperContent>(content);
            })
        {
            TagMode = tagMode
        };
        return output;
    }

    #endregion

    #region Output Content Extraction

    /// <summary>
    /// Extracts the HTML content from a TagHelperOutput.
    /// </summary>
    /// <param name="output">The TagHelperOutput to extract content from</param>
    /// <returns>The HTML content as a string</returns>
    protected static string GetOutputContent(TagHelperOutput output)
    {
        using var writer = new StringWriter();
        output.Content.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }

    /// <summary>
    /// Extracts the pre-content HTML from a TagHelperOutput.
    /// </summary>
    /// <param name="output">The TagHelperOutput to extract pre-content from</param>
    /// <returns>The pre-content HTML as a string</returns>
    protected static string GetPreContent(TagHelperOutput output)
    {
        using var writer = new StringWriter();
        output.PreContent.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }

    /// <summary>
    /// Extracts the post-content HTML from a TagHelperOutput.
    /// </summary>
    /// <param name="output">The TagHelperOutput to extract post-content from</param>
    /// <returns>The post-content HTML as a string</returns>
    protected static string GetPostContent(TagHelperOutput output)
    {
        using var writer = new StringWriter();
        output.PostContent.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }

    #endregion

    #region ModelExpression Creation

    /// <summary>
    /// Creates a strongly-typed ModelExpression for testing.
    /// </summary>
    /// <typeparam name="TModel">The model type</typeparam>
    /// <typeparam name="TProperty">The property type</typeparam>
    /// <param name="model">The model instance</param>
    /// <param name="expression">Lambda expression pointing to the property</param>
    /// <param name="value">The property value</param>
    /// <returns>Configured ModelExpression instance</returns>
    protected static ModelExpression CreateModelExpression<TModel, TProperty>(
        TModel model,
        System.Linq.Expressions.Expression<Func<TModel, TProperty>> expression,
        TProperty value)
    {
        var metadataProvider = new EmptyModelMetadataProvider();
        var modelType = typeof(TModel);

        // Get property name from expression
        var memberExpression = (System.Linq.Expressions.MemberExpression)expression.Body;
        var propertyName = memberExpression.Member.Name;

        // Create model explorer for the property
        var containerMetadata = metadataProvider.GetMetadataForType(modelType);
        var modelExplorer = new ModelExplorer(metadataProvider, containerMetadata, model)
            .GetExplorerForProperty(propertyName, value);

        return new ModelExpression(propertyName, modelExplorer);
    }

    /// <summary>
    /// Creates a simple ModelExpression with minimal metadata for testing.
    /// </summary>
    /// <param name="propertyName">The property name</param>
    /// <param name="value">The property value</param>
    /// <param name="modelType">Optional model type (defaults to object)</param>
    /// <returns>Configured ModelExpression instance</returns>
    protected static ModelExpression CreateSimpleModelExpression(
        string propertyName,
        object? value,
        Type? modelType = null)
    {
        modelType ??= typeof(object);
        var metadataProvider = new EmptyModelMetadataProvider();
        var containerMetadata = metadataProvider.GetMetadataForType(modelType);

        var modelExplorer = new ModelExplorer(
            metadataProvider,
            containerMetadata,
            model: null);

        return new ModelExpression(propertyName, modelExplorer);
    }

    #endregion

    #region Attribute Creation

    /// <summary>
    /// Creates a TagHelperAttributeList with the specified attributes.
    /// </summary>
    /// <param name="attributes">Key-value pairs of attribute names and values</param>
    /// <returns>Configured TagHelperAttributeList</returns>
    protected static TagHelperAttributeList CreateAttributes(params (string name, object value)[] attributes)
    {
        var attributeList = new TagHelperAttributeList();
        foreach (var (name, value) in attributes)
        {
            attributeList.Add(name, value);
        }
        return attributeList;
    }

    #endregion
}
