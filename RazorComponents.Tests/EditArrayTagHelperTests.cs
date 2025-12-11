using System.Reflection;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using RazorComponents.TagHelpers.EditArray;
using Xunit;

namespace RazorComponents.Tests;

public partial class EditArrayTagHelperTests
{
    #region Test Models
    
    private class TestModel
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    private class TestModelWithIsDeleted
    {
        public string Name { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
    }
    
    #endregion

    #region Helper Methods

    private static EditArrayTagHelper CreateTagHelper(
        IHtmlHelper? htmlHelper = null,
        string viewName = "EditorTemplate",
        IEnumerable<object>? items = null,
        ViewContext? viewContext = null,
        string? id = null)
    {
        htmlHelper ??= CreateMockHtmlHelper().Object;
        viewContext ??= CreateViewContext();
        
        return new EditArrayTagHelper(htmlHelper)
        {
            ViewName = viewName,
            Items = items ?? new List<object>(),
            ViewContext = viewContext,
            Id = id ?? "test"
        };
    }

    private static Mock<IHtmlHelper> CreateMockHtmlHelper(string partialContent = "<div>Test Content</div>")
    {
        var mockHtmlHelper = new Mock<IHtmlHelper>();
        
        // Setup PartialAsync to return test content
        mockHtmlHelper
            .Setup(h => h.PartialAsync(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<ViewDataDictionary>()))
            .ReturnsAsync(new HtmlString(partialContent));

        // Setup IViewContextAware.Contextualize
        mockHtmlHelper
            .As<IViewContextAware>()
            .Setup(h => h.Contextualize(It.IsAny<ViewContext>()));

        return mockHtmlHelper;
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
        string tagName = "edit-array",
        TagHelperAttributeList? attributes = null)
    {
        return new TagHelperContext(
            tagName: tagName,
            allAttributes: attributes ?? new TagHelperAttributeList(),
            items: new Dictionary<object, object>(),
            uniqueId: "test");
    }

    private static TagHelperOutput CreateOutput(
        string tagName = "edit-array",
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
        using (var writer = new StringWriter())
        {
            output.Content.WriteTo(writer, HtmlEncoder.Default);
            return writer.ToString();
        }
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidHtmlHelper_InitializesCorrectly()
    {
        // Arrange
        var mockHtmlHelper = new Mock<IHtmlHelper>();

        // Act
        var tagHelper = new EditArrayTagHelper(mockHtmlHelper.Object)
        {
            ViewName = "TestView",
            Items = new List<object>(),
            ViewContext = CreateViewContext()
        };

        // Assert
        Assert.NotNull(tagHelper);
        Assert.Equal("TestView", tagHelper.ViewName);
        Assert.Empty(tagHelper.Items);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void ViewName_CanBeSetAndRetrieved()
    {
        // Arrange
        var tagHelper = CreateTagHelper();

        // Act
        tagHelper.ViewName = "CustomView";

        // Assert
        Assert.Equal("CustomView", tagHelper.ViewName);
    }

    [Fact]
    public void DisplayViewName_DefaultsToNull()
    {
        // Arrange & Act
        var tagHelper = CreateTagHelper();

        // Assert
        Assert.Null(tagHelper.DisplayViewName);
    }

    [Fact]
    public void DisplayViewName_CanBeSetAndRetrieved()
    {
        // Arrange
        var tagHelper = CreateTagHelper();

        // Act
        tagHelper.DisplayViewName = "DisplayView";

        // Assert
        Assert.Equal("DisplayView", tagHelper.DisplayViewName);
    }

    [Fact]
    public void Items_CanBeSetAndRetrieved()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper();

        // Act
        tagHelper.Items = items;

        // Assert
        Assert.Equal(items, tagHelper.Items);
    }

    [Fact]
    public void For_DefaultsToNull()
    {
        // Arrange & Act
        var tagHelper = CreateTagHelper();

        // Assert
        Assert.Null(tagHelper.For);
    }

    [Fact]
    public void RenderTemplate_DefaultsToFalse()
    {
        // Arrange & Act
        var tagHelper = CreateTagHelper();

        // Assert
        Assert.False(tagHelper.RenderTemplate);
    }

    [Fact]
    public void ShowAddButton_DefaultsToFalse()
    {
        // Arrange & Act
        var tagHelper = CreateTagHelper();

        // Assert
        Assert.False(tagHelper.ShowAddButton);
    }

    [Fact]
    public void DisplayMode_DefaultsToFalse()
    {
        // Arrange & Act
        var tagHelper = CreateTagHelper();

        // Assert
        Assert.False(tagHelper.DisplayMode);
    }

    [Fact]
    public void OnUpdate_DefaultsToNull()
    {
        // Arrange & Act
        var tagHelper = CreateTagHelper();

        // Assert
        Assert.Null(tagHelper.OnUpdate);
    }

    [Fact]
    public void ContainerCssClass_DefaultsToEditArrayContainer()
    {
        // Arrange & Act
        var tagHelper = CreateTagHelper();

        // Assert
        Assert.Equal("edit-array-container", tagHelper.ContainerCssClass);
    }

    [Fact]
    public void ContainerCssClass_CanBeSetAndRetrieved()
    {
        // Arrange
        var tagHelper = CreateTagHelper();

        // Act
        tagHelper.ContainerCssClass = "my-custom-container";

        // Assert
        Assert.Equal("my-custom-container", tagHelper.ContainerCssClass);
    }

    [Fact]
    public void ItemCssClass_DefaultsToEditArrayItem()
    {
        // Arrange & Act
        var tagHelper = CreateTagHelper();

        // Assert
        Assert.Equal("edit-array-item", tagHelper.ItemCssClass);
    }

    [Fact]
    public void ItemCssClass_CanBeSetAndRetrieved()
    {
        // Arrange
        var tagHelper = CreateTagHelper();

        // Act
        tagHelper.ItemCssClass = "my-item-class";

        // Assert
        Assert.Equal("my-item-class", tagHelper.ItemCssClass);
    }

    [Fact]
    public void ButtonCssClass_DefaultsToBtn()
    {
        // Arrange & Act
        var tagHelper = CreateTagHelper();

        // Assert
        Assert.Equal("btn", tagHelper.ButtonCssClass);
    }

    [Fact]
    public void ButtonCssClass_CanBeSetAndRetrieved()
    {
        // Arrange
        var tagHelper = CreateTagHelper();

        // Act
        tagHelper.ButtonCssClass = "custom-button-class";

        // Assert
        Assert.Equal("custom-button-class", tagHelper.ButtonCssClass);
    }

    #endregion

    #region GetFieldName Tests

    [Fact]
    public void GetFieldName_WithNoPrefixAndNoCollectionName_ReturnsIndex()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var method = typeof(EditArrayTagHelper).GetMethod("GetFieldName",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        var result = method?.Invoke(tagHelper, new object?[] { null, string.Empty, 0 });

        // Assert
        Assert.Equal("[0]", result?.ToString());
    }

    [Fact]
    public void GetFieldName_WithPrefixAndNoCollectionName_ReturnsPrefixWithIndex()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var method = typeof(EditArrayTagHelper).GetMethod("GetFieldName",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        var result = method?.Invoke(tagHelper, new object?[] { "model", string.Empty, 1 });

        // Assert
        Assert.Equal("model[1]", result?.ToString());
    }

    [Fact]
    public void GetFieldName_WithCollectionName_ReturnsCollectionNameWithIndex()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var method = typeof(EditArrayTagHelper).GetMethod("GetFieldName",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        var result = method?.Invoke(tagHelper, new object?[] { null, "Items", 0 });

        // Assert
        Assert.Equal("Items[0]", result?.ToString());
    }

    [Fact]
    public void GetFieldName_WithPrefixAndCollectionName_ReturnsPrefixDotCollectionNameWithIndex()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var method = typeof(EditArrayTagHelper).GetMethod("GetFieldName",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        var result = method?.Invoke(tagHelper, new object?[] { "model", "Items", 2 });

        // Assert
        Assert.Equal("model.Items[2]", result?.ToString());
    }

    [Theory]
    [InlineData(null, "", 0, "[0]")]
    [InlineData("", "", 5, "[5]")]
    [InlineData("prefix", "", 3, "prefix[3]")]
    [InlineData(null, "Collection", 1, "Collection[1]")]
    [InlineData("", "Collection", 2, "Collection[2]")]
    [InlineData("prefix", "Collection", 4, "prefix.Collection[4]")]
    public void GetFieldName_WithVariousInputs_ReturnsExpectedFormat(
        string? prefix, string collectionName, int index, string expected)
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var method = typeof(EditArrayTagHelper).GetMethod("GetFieldName",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        var result = method?.Invoke(tagHelper, new object?[] { prefix, collectionName, index });

        // Assert
        Assert.Equal(expected, result?.ToString());
    }

    #endregion

    #region ProcessAsync - Basic Functionality Tests

    [Fact]
    public async Task ProcessAsync_WithEmptyItems_GeneratesBasicStructure()
    {
        // Arrange
        var tagHelper = CreateTagHelper(items: new List<object>());
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("div", output.TagName);
        Assert.Equal("edit-array-container", output.Attributes["class"].Value);
        Assert.Equal("edit-array-test", output.Attributes["id"].Value);
        
        var content = GetOutputContent(output);
        Assert.Contains("<div class=\"edit-array-items\" id=\"edit-array-test-items\">", content);
        Assert.Contains("</div>", content);
    }

    [Fact]
    public async Task ProcessAsync_WithSingleItem_RendersOneItem()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test1" } };
        var tagHelper = CreateTagHelper(items: items);
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("<div class=\"edit-array-item\"", content);
        Assert.Contains("Test Content", content);
    }

    #endregion
}
