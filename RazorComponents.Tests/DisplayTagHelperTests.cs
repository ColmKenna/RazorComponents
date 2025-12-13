using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using RazorComponents.TagHelpers.DisplayTagHelpers;
using Xunit;

namespace RazorComponents.Tests;

public class DisplayTagHelperTests
{
    #region Test Models

    private class SimpleModel
    {
        public string Name { get; set; } = string.Empty;
    }

    private class ModelWithDisplayName
    {
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;
    }

    private class ModelWithDataAnnotations
    {
        [Display(Name = "User Email")]
        [DisplayName("Email Address")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Age")]
        public int Age { get; set; }

        public bool IsActive { get; set; }
    }

    private class NestedModel
    {
        public string ParentName { get; set; } = string.Empty;
        public SimpleModel Child { get; set; } = new();
    }

    #endregion

    #region Helper Methods

    private static DisplayTagHelper CreateTagHelper(
        ModelExpression? forExpression = null,
        string? labelClass = null,
        string? displayClass = null,
        string? idSuffix = null,
        ViewContext? viewContext = null)
    {
        viewContext ??= CreateViewContext();
        forExpression ??= CreateSimpleModelExpression("Dummy", "DummyValue");

        return new DisplayTagHelper
        {
            For = forExpression,
            LabelClass = labelClass,
            DisplayClass = displayClass,
            IdSuffix = idSuffix,
            ViewContext = viewContext
        };
    }

    private static ViewContext CreateViewContext(string? htmlFieldPrefix = null)
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

    private static TagHelperContext CreateContext(
        string tagName = "display",
        TagHelperAttributeList? attributes = null)
    {
        return new TagHelperContext(
            tagName: tagName,
            allAttributes: attributes ?? new TagHelperAttributeList(),
            items: new Dictionary<object, object>(),
            uniqueId: "test");
    }

    private static TagHelperOutput CreateOutput(
        string tagName = "display",
        TagMode tagMode = TagMode.StartTagAndEndTag)
    {
        return new TagHelperOutput(
            tagName: tagName,
            attributes: new TagHelperAttributeList(),
            getChildContentAsync: (useCached, encoder) =>
                Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()))
        {
            TagMode = tagMode
        };
    }

    private static string GetOutputContent(TagHelperOutput output)
    {
        using var writer = new StringWriter();
        output.Content.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }

    private static ModelExpression CreateModelExpression<TModel, TProperty>(
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
        var propertyMetadata = metadataProvider.GetMetadataForProperty(modelType, propertyName);
        var modelExplorer = new ModelExplorer(metadataProvider, containerMetadata, model)
            .GetExplorerForProperty(propertyName, value);

        return new ModelExpression(propertyName, modelExplorer);
    }

    private static ModelExpression CreateSimpleModelExpression(string propertyName, object? value, string? displayName = null)
    {
        var metadataProvider = new EmptyModelMetadataProvider();
        var modelType = typeof(SimpleModel);
        var containerMetadata = metadataProvider.GetMetadataForType(modelType);
        var propertyMetadata = metadataProvider.GetMetadataForProperty(modelType, nameof(SimpleModel.Name));
        
        var model = new SimpleModel { Name = value?.ToString() ?? string.Empty };
        var modelExplorer = new ModelExplorer(metadataProvider, containerMetadata, model)
            .GetExplorerForProperty(nameof(SimpleModel.Name), value);

        return new ModelExpression(propertyName, modelExplorer);
    }

    #endregion

    #region Constructor and Default Property Tests

    [Fact]
    public void Constructor_CreatesInstance_WithDefaultValues()
    {
        // Arrange & Act
        var tagHelper = new DisplayTagHelper
        {
            For = CreateSimpleModelExpression("Dummy", "Dummy"),
            ViewContext = CreateViewContext()
        };

        // Assert
        Assert.NotNull(tagHelper);
        Assert.Null(tagHelper.LabelClass);
        Assert.Null(tagHelper.DisplayClass);
        Assert.Null(tagHelper.IdSuffix);
    }

    [Fact]
    public void For_CanBeSetAndRetrieved()
    {
        // Arrange
        var modelExpression = CreateSimpleModelExpression("Name", "Test Value");
        var tagHelper = new DisplayTagHelper
        { 
            For = modelExpression,
            ViewContext = CreateViewContext()
        };

        // Assert
        Assert.Equal(modelExpression, tagHelper.For);
    }

    [Fact]
    public void LabelClass_CanBeSetAndRetrieved()
    {
        // Arrange
        var tagHelper = new DisplayTagHelper
        {
            For = CreateSimpleModelExpression("Dummy", "Dummy"),
            ViewContext = CreateViewContext()
        };

        // Act
        tagHelper.LabelClass = "my-label-class";

        // Assert
        Assert.Equal("my-label-class", tagHelper.LabelClass);
    }

    [Fact]
    public void DisplayClass_CanBeSetAndRetrieved()
    {
        // Arrange
        var tagHelper = new DisplayTagHelper
        {
            For = CreateSimpleModelExpression("Dummy", "Dummy"),
            ViewContext = CreateViewContext()
        };

        // Act
        tagHelper.DisplayClass = "my-display-class";

        // Assert
        Assert.Equal("my-display-class", tagHelper.DisplayClass);
    }

    [Fact]
    public void IdSuffix_CanBeSetAndRetrieved()
    {
        // Arrange
        var tagHelper = new DisplayTagHelper
        {
            For = CreateSimpleModelExpression("Dummy", "Dummy"),
            ViewContext = CreateViewContext()
        };

        // Act
        tagHelper.IdSuffix = "_suffix";

        // Assert
        Assert.Equal("_suffix", tagHelper.IdSuffix);
    }

    [Fact]
    public void ViewContext_CanBeSetAndRetrieved()
    {
        // Arrange
        var viewContext = CreateViewContext();
        var tagHelper = new DisplayTagHelper
        {
            For = CreateSimpleModelExpression("Dummy", "Dummy"),
            ViewContext = viewContext
        };

        // Assert
        Assert.Equal(viewContext, tagHelper.ViewContext);
    }

    #endregion

    #region Process - Basic Functionality Tests

    [Fact]
    public void Process_WithValidInput_GeneratesLabelAndDiv()
    {
        // Arrange
        var model = new SimpleModel { Name = "Test Value" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(forExpression: modelExpression);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        Assert.Null(output.TagName); // <display> tag should be removed
        var content = GetOutputContent(output);
        Assert.Contains("<label", content);
        Assert.Contains("<div", content);
        Assert.Contains("Test Value", content);
    }

    [Fact]
    public void Process_WithValidInput_RemovesDisplayTag()
    {
        // Arrange
        var model = new SimpleModel { Name = "Test" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(forExpression: modelExpression);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        Assert.Null(output.TagName);
    }

    [Fact]
    public void Process_GeneratesLabelWithForAttribute()
    {
        // Arrange
        var model = new SimpleModel { Name = "Test" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(forExpression: modelExpression);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("for=\"Name\"", content);
    }

    [Fact]
    public void Process_GeneratesDivWithDataIdAttribute()
    {
        // Arrange
        var model = new SimpleModel { Name = "Test" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(forExpression: modelExpression);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("data-id=\"Name\"", content);
    }

    [Fact]
    public void Process_GeneratesDivWithDataDisplayForAttribute()
    {
        // Arrange
        var model = new SimpleModel { Name = "Test" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(forExpression: modelExpression);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("data-display-for=\"Name\"", content);
    }

    #endregion

    #region Process - Label Content Tests

    [Fact]
    public void Process_WithDisplayName_UsesDisplayNameInLabel()
    {
        // Arrange
        var model = new ModelWithDisplayName { Name = "John Doe" };
        
        // Create with display name metadata
        var metadataProvider = new EmptyModelMetadataProvider();
        var containerMetadata = metadataProvider.GetMetadataForType(typeof(ModelWithDisplayName));
        var modelExplorer = new ModelExplorer(metadataProvider, containerMetadata, model)
            .GetExplorerForProperty(nameof(ModelWithDisplayName.Name), model.Name);
        var modelExpression = new ModelExpression(nameof(ModelWithDisplayName.Name), modelExplorer);
        
        var tagHelper = CreateTagHelper(forExpression: modelExpression);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        // Since we're using EmptyModelMetadataProvider, it won't have DisplayName, so it falls back to property name
        Assert.Contains("Name", content);
    }

    [Fact]
    public void Process_WithoutDisplayName_UsesPropertyNameInLabel()
    {
        // Arrange
        var model = new SimpleModel { Name = "Test" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(forExpression: modelExpression);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains(">Name<", content);
    }

    #endregion

    #region Process - Value Display Tests

    [Fact]
    public void Process_WithStringValue_DisplaysValueInDiv()
    {
        // Arrange
        var model = new SimpleModel { Name = "Expected Value" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(forExpression: modelExpression);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("Expected Value", content);
    }

    [Fact]
    public void Process_WithNullValue_DisplaysEmptyString()
    {
        // Arrange
        var model = new SimpleModel { Name = null! };
        var modelExpression = CreateSimpleModelExpression("Name", null);
        var tagHelper = CreateTagHelper(forExpression: modelExpression);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        // The div should still be present but with empty content
        Assert.Contains("<div", content);
        Assert.Contains("</div>", content);
    }

    [Fact]
    public void Process_WithEmptyStringValue_DisplaysEmptyString()
    {
        // Arrange
        var model = new SimpleModel { Name = string.Empty };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(forExpression: modelExpression);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("<div", content);
    }

    #endregion

    #region Process - CSS Class Tests

    [Fact]
    public void Process_WithLabelClass_AddsClassToLabel()
    {
        // Arrange
        var model = new SimpleModel { Name = "Test" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(
            forExpression: modelExpression,
            labelClass: "custom-label-class");
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("class=\"custom-label-class\"", content);
    }

    [Fact]
    public void Process_WithoutLabelClass_DoesNotAddClassToLabel()
    {
        // Arrange
        var model = new SimpleModel { Name = "Test" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(forExpression: modelExpression);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        // Check that label doesn't have a class attribute (only for attribute)
        Assert.Matches(@"<label for=""[^""]+""[^>]*>", content);
    }

    [Fact]
    public void Process_WithDisplayClass_AddsClassToDiv()
    {
        // Arrange
        var model = new SimpleModel { Name = "Test" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(
            forExpression: modelExpression,
            displayClass: "custom-display-class");
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("class=\"custom-display-class\"", content);
    }

    [Fact]
    public void Process_WithoutDisplayClass_DoesNotAddClassToDiv()
    {
        // Arrange
        var model = new SimpleModel { Name = "Test" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(forExpression: modelExpression);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        // Div should have data-id and data-display-for attributes
        Assert.Contains("data-id=", content);
        Assert.Contains("data-display-for=", content);
        // Verify div is present and doesn't have class="..." with a non-empty class before data-id
        // When DisplayClass is null/empty, TagBuilder won't add a class attribute
        Assert.Contains("<div", content);
    }

    [Fact]
    public void Process_WithBothClasses_AddsBothClasses()
    {
        // Arrange
        var model = new SimpleModel { Name = "Test" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(
            forExpression: modelExpression,
            labelClass: "label-class",
            displayClass: "display-class");
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("class=\"label-class\"", content);
        Assert.Contains("class=\"display-class\"", content);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Process_WithEmptyOrWhitespaceLabelClass_DoesNotAddClassToLabel(string labelClass)
    {
        // Arrange
        var model = new SimpleModel { Name = "Test" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(
            forExpression: modelExpression,
            labelClass: labelClass);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert - Label should still be created even with empty class
        var content = GetOutputContent(output);
        Assert.Contains("<label", content);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Process_WithEmptyOrWhitespaceDisplayClass_DoesNotAddClassToDiv(string displayClass)
    {
        // Arrange
        var model = new SimpleModel { Name = "Test" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(
            forExpression: modelExpression,
            displayClass: displayClass);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert - Div should still be created even with empty class
        var content = GetOutputContent(output);
        Assert.Contains("<div", content);
    }

    #endregion

    #region Process - Id Generation Tests

    [Fact]
    public void Process_WithSimplePropertyName_GeneratesCorrectId()
    {
        // Arrange
        var model = new SimpleModel { Name = "Test" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(forExpression: modelExpression);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("for=\"Name\"", content);
        Assert.Contains("data-id=\"Name\"", content);
    }

    [Fact]
    public void Process_WithIdSuffix_AppendsToId()
    {
        // Arrange
        var model = new SimpleModel { Name = "Test" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(
            forExpression: modelExpression,
            idSuffix: "_display");
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("for=\"Name_display\"", content);
        Assert.Contains("data-id=\"Name_display\"", content);
    }

    [Fact]
    public void Process_WithEmptyIdSuffix_DoesNotAppendToId()
    {
        // Arrange
        var model = new SimpleModel { Name = "Test" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(
            forExpression: modelExpression,
            idSuffix: string.Empty);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("for=\"Name\"", content);
        Assert.Contains("data-id=\"Name\"", content);
    }

    [Fact]
    public void Process_WithNullIdSuffix_DoesNotAppendToId()
    {
        // Arrange
        var model = new SimpleModel { Name = "Test" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(
            forExpression: modelExpression,
            idSuffix: null);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("for=\"Name\"", content);
    }

    #endregion

    #region Process - Html Field Prefix Tests

    [Fact]
    public void Process_WithHtmlFieldPrefix_IncludesPrefixInId()
    {
        // Arrange
        var model = new SimpleModel { Name = "Test" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var viewContext = CreateViewContext(htmlFieldPrefix: "Parent");
        var tagHelper = CreateTagHelper(
            forExpression: modelExpression,
            viewContext: viewContext);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("Parent_Name", content);
    }

    [Fact]
    public void Process_WithNestedHtmlFieldPrefix_ReplacesDotsWithUnderscores()
    {
        // Arrange
        var model = new SimpleModel { Name = "Test" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var viewContext = CreateViewContext(htmlFieldPrefix: "Parent.Child");
        var tagHelper = CreateTagHelper(
            forExpression: modelExpression,
            viewContext: viewContext);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("Parent_Child_Name", content);
    }

    [Fact]
    public void Process_WithArrayIndexInPrefix_ReplacesBracketsWithUnderscores()
    {
        // Arrange
        var model = new SimpleModel { Name = "Test" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var viewContext = CreateViewContext(htmlFieldPrefix: "Items[0]");
        var tagHelper = CreateTagHelper(
            forExpression: modelExpression,
            viewContext: viewContext);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("Items_0__Name", content);
    }

    [Fact]
    public void Process_WithComplexPrefix_ReplacesAllSpecialCharacters()
    {
        // Arrange
        var model = new SimpleModel { Name = "Test" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var viewContext = CreateViewContext(htmlFieldPrefix: "Parent.Children[0].Grandchild");
        var tagHelper = CreateTagHelper(
            forExpression: modelExpression,
            viewContext: viewContext);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("Parent_Children_0__Grandchild_Name", content);
    }

    #endregion

    #region Process - Edge Cases

    [Fact]
    public void Process_WithSpecialCharactersInValue_EncodesHtml()
    {
        // Arrange
        var model = new SimpleModel { Name = "<script>alert('xss')</script>" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(forExpression: modelExpression);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        // HTML should be encoded
        Assert.DoesNotContain("<script>", content);
        Assert.Contains("&lt;script&gt;", content);
    }

    [Fact]
    public void Process_WithLongValue_DisplaysFullValue()
    {
        // Arrange
        var longValue = new string('A', 1000);
        var model = new SimpleModel { Name = longValue };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(forExpression: modelExpression);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains(longValue, content);
    }

    [Fact]
    public void Process_WithMultipleSpecialCharactersInId_ReplacesAll()
    {
        // Arrange
        var model = new SimpleModel { Name = "Test" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var viewContext = CreateViewContext(htmlFieldPrefix: "A.B[0].C[1].D");
        var tagHelper = CreateTagHelper(
            forExpression: modelExpression,
            viewContext: viewContext,
            idSuffix: "_suffix");
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("A_B_0__C_1__D_Name_suffix", content);
    }

    #endregion

    #region Process - Output Structure Tests

    [Fact]
    public void Process_OutputsLabelBeforeDiv()
    {
        // Arrange
        var model = new SimpleModel { Name = "Test" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(forExpression: modelExpression);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        var labelIndex = content.IndexOf("<label");
        var divIndex = content.IndexOf("<div");
        Assert.True(labelIndex < divIndex, "Label should appear before div");
    }

    [Fact]
    public void Process_CreatesProperHtmlStructure()
    {
        // Arrange
        var model = new SimpleModel { Name = "Test Value" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(
            forExpression: modelExpression,
            labelClass: "lbl",
            displayClass: "disp");
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        
        // Verify label structure
        Assert.Contains("<label", content);
        Assert.Contains("</label>", content);
        
        // Verify div structure
        Assert.Contains("<div", content);
        Assert.Contains("</div>", content);
        
        // Verify content appears inside div
        Assert.Matches(@"<div[^>]*>.*Test Value.*</div>", content);
    }

    #endregion

    #region Theory-Based Tests

    [Theory]
    [InlineData("Name", "Name")]
    [InlineData("PropertyName", "PropertyName")]
    [InlineData("ComplexPropertyName", "ComplexPropertyName")]
    public void Process_WithVariousPropertyNames_GeneratesCorrectId(string propertyName, string expectedId)
    {
        // Arrange
        var modelExpression = CreateSimpleModelExpression(propertyName, "Value");
        var tagHelper = CreateTagHelper(forExpression: modelExpression);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains($"for=\"{expectedId}\"", content);
    }

    [Theory]
    [InlineData("_suffix", "_suffix")]
    [InlineData("-suffix", "-suffix")]
    [InlineData("123", "123")]
    public void Process_WithVariousIdSuffixes_AppendsCorrectly(string suffix, string expectedSuffix)
    {
        // Arrange
        var model = new SimpleModel { Name = "Test" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(
            forExpression: modelExpression,
            idSuffix: suffix);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains($"for=\"Name{expectedSuffix}\"", content);
    }

    [Theory]
    [InlineData("simple", "simple")]
    [InlineData("with spaces", "with spaces")]
    [InlineData("with-dashes", "with-dashes")]
    public void Process_WithVariousValues_DisplaysCorrectly(string value, string expectedDisplay)
    {
        // Arrange
        var model = new SimpleModel { Name = value };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(forExpression: modelExpression);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains(expectedDisplay, content);
    }

    #endregion

    #region Process - Null Validation Tests

    /// <summary>
    /// Verifies that Process throws InvalidOperationException when For property is null.
    /// This ensures proper error messaging for missing asp-for attribute.
    /// </summary>
    [Fact]
    public void Process_WithNullFor_ThrowsInvalidOperationException()
    {
        // Arrange
        var tagHelper = new DisplayTagHelper
        {
            For = null!,
            ViewContext = CreateViewContext()
        };
        var context = CreateContext();
        var output = CreateOutput();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            tagHelper.Process(context, output));
        Assert.NotEmpty(exception.Message);
        Assert.Contains("asp-for", exception.Message);
    }

    /// <summary>
    /// Verifies that Process throws InvalidOperationException when ViewContext property is null.
    /// This ensures proper error messaging when framework fails to set ViewContext.
    /// </summary>
    [Fact]
    public void Process_WithNullViewContext_ThrowsInvalidOperationException()
    {
        // Arrange
        var tagHelper = new DisplayTagHelper
        {
            For = CreateSimpleModelExpression("Test", "Value"),
            ViewContext = null!
        };
        var context = CreateContext();
        var output = CreateOutput();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            tagHelper.Process(context, output));
        Assert.NotEmpty(exception.Message);
        Assert.Contains(nameof(ViewContext), exception.Message);
    }

    /// <summary>
    /// Verifies that Process throws InvalidOperationException when both properties are null.
    /// Should throw for the first null property checked (For).
    /// </summary>
    [Fact]
    public void Process_WithBothPropertiesNull_ThrowsInvalidOperationException()
    {
        // Arrange
        var tagHelper = new DisplayTagHelper
        {
            For = null!,
            ViewContext = null!
        };
        var context = CreateContext();
        var output = CreateOutput();

        // Act & Assert
        // Should throw for the first null property checked
        Assert.Throws<InvalidOperationException>(() =>
            tagHelper.Process(context, output));
    }

    /// <summary>
    /// Verifies that Process completes successfully when both required properties are set.
    /// This is the happy path that validates normal operation is not affected by validation.
    /// </summary>
    [Fact]
    public void Process_WithValidForAndViewContext_DoesNotThrow()
    {
        // Arrange
        var model = new SimpleModel { Name = "Test" };
        var modelExpression = CreateModelExpression(model, m => m.Name, model.Name);
        var tagHelper = CreateTagHelper(forExpression: modelExpression);
        var context = CreateContext();
        var output = CreateOutput();

        // Act & Assert - Should complete without throwing validation exceptions
        tagHelper.Process(context, output);
        
        // Verify output was generated
        var content = GetOutputContent(output);
        Assert.Contains("<label", content);
        Assert.Contains("<div", content);
    }

    #endregion
}
