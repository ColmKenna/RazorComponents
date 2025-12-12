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

    [Fact]
    public void EmptyPlaceholder_DefaultsToNull()
    {
        // Arrange & Act
        var tagHelper = CreateTagHelper();

        // Assert
        Assert.Null(tagHelper.EmptyPlaceholder);
    }

    [Fact]
    public void EmptyPlaceholder_CanBeSetAndRetrieved()
    {
        // Arrange
        var tagHelper = CreateTagHelper();

        // Act
        tagHelper.EmptyPlaceholder = "No items";

        // Assert
        Assert.Equal("No items", tagHelper.EmptyPlaceholder);
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
    
    #region ProcessAsync - Empty Placeholder Tests

    [Fact]
    public async Task ProcessAsync_WithEmptyItemsAndPlaceholder_RendersPlaceholder()
    {
        // Arrange
        var tagHelper = CreateTagHelper(items: new List<object>());
        tagHelper.EmptyPlaceholder = "No items to display";
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("No items to display", content);
    }

    [Fact]
    public async Task ProcessAsync_WithItemsAndPlaceholder_DoesNotRenderPlaceholder()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test1" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.EmptyPlaceholder = "No items to display";
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.DoesNotContain("No items to display", content);
    }

    #endregion
    
    #region ProcessAsync - Validation Tests
    
    [Fact]
    public async Task ProcessAsync_WithNullViewName_ThrowsInvalidOperationException()
    {
        // Arrange
        var tagHelper = new EditArrayTagHelper(CreateMockHtmlHelper().Object)
        {
            ViewName = "temp",
            Items = new List<object>(),
            ViewContext = CreateViewContext()
        };
        
        // Use reflection to set ViewName to null (bypassing required property validation)
        var viewNameProperty = typeof(EditArrayTagHelper).GetProperty(nameof(EditArrayTagHelper.ViewName));
        viewNameProperty?.SetValue(tagHelper, null);
        
        var context = CreateContext();
        var output = CreateOutput();
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            tagHelper.ProcessAsync(context, output));
        
        Assert.Contains("ViewName", exception.Message);
        Assert.Contains("required", exception.Message);
    }
    
    [Fact]
    public async Task ProcessAsync_WithEmptyViewName_ThrowsInvalidOperationException()
    {
        // Arrange
        var tagHelper = new EditArrayTagHelper(CreateMockHtmlHelper().Object)
        {
            ViewName = string.Empty,
            Items = new List<object>(),
            ViewContext = CreateViewContext()
        };
        var context = CreateContext();
        var output = CreateOutput();
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            tagHelper.ProcessAsync(context, output));
        
        Assert.Contains("ViewName", exception.Message);
        Assert.Contains("required", exception.Message);
    }
    
    [Fact]
    public async Task ProcessAsync_WithWhitespaceViewName_ThrowsInvalidOperationException()
    {
        // Arrange
        var tagHelper = new EditArrayTagHelper(CreateMockHtmlHelper().Object)
        {
            ViewName = "   ",
            Items = new List<object>(),
            ViewContext = CreateViewContext()
        };
        var context = CreateContext();
        var output = CreateOutput();
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            tagHelper.ProcessAsync(context, output));
        
        Assert.Contains("ViewName", exception.Message);
        Assert.Contains("required", exception.Message);
    }
    
    [Fact]
    public async Task ProcessAsync_WithNullItems_ThrowsInvalidOperationException()
    {
        // Arrange
        var tagHelper = new EditArrayTagHelper(CreateMockHtmlHelper().Object)
        {
            ViewName = "EditorTemplate",
            Items = new List<object>(),
            ViewContext = CreateViewContext()
        };
        
        // Use reflection to set Items to null (bypassing required property validation)
        var itemsProperty = typeof(EditArrayTagHelper).GetProperty(nameof(EditArrayTagHelper.Items));
        itemsProperty?.SetValue(tagHelper, null);
        
        var context = CreateContext();
        var output = CreateOutput();
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            tagHelper.ProcessAsync(context, output));
        
        Assert.Contains("Items", exception.Message);
        Assert.Contains("required", exception.Message);
    }
    
    [Fact]
    public async Task ProcessAsync_WithNullViewContext_ThrowsInvalidOperationException()
    {
        // Arrange
        var tagHelper = new EditArrayTagHelper(CreateMockHtmlHelper().Object)
        {
            ViewName = "EditorTemplate",
            Items = new List<object>(),
            ViewContext = CreateViewContext(),
            Id = "test"  // Set Id so we pass Id validation and reach ViewContext validation
        };

        // Use reflection to set ViewContext to null (bypassing required property validation)
        var viewContextProperty = typeof(EditArrayTagHelper).GetProperty(nameof(EditArrayTagHelper.ViewContext));
        viewContextProperty?.SetValue(tagHelper, null);

        var context = CreateContext();
        var output = CreateOutput();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            tagHelper.ProcessAsync(context, output));

        Assert.Contains("ViewContext", exception.Message);
        Assert.Contains("required", exception.Message);
    }
    
    [Fact]
    public async Task ProcessAsync_WithNullViewData_ThrowsInvalidOperationException()
    {
        // Arrange
        var viewContext = CreateViewContext();
        viewContext.ViewData = null!; // Force null ViewData

        var tagHelper = new EditArrayTagHelper(CreateMockHtmlHelper().Object)
        {
            ViewName = "EditorTemplate",
            Items = new List<object>(),
            ViewContext = viewContext,
            Id = "test"  // Set Id so we pass Id validation and reach ViewData validation
        };
        var context = CreateContext();
        var output = CreateOutput();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            tagHelper.ProcessAsync(context, output));

        Assert.Contains("ViewData", exception.Message);
    }
    
    
    [Fact]
    public async Task ProcessAsync_WithEmptyItems_DoesNotThrow()
    {
        // Arrange
        var tagHelper = CreateTagHelper(items: new List<object>());
        var context = CreateContext();
        var output = CreateOutput();

        // Act & Assert - Should not throw, empty collection is valid
        await tagHelper.ProcessAsync(context, output);

        // Verify it rendered successfully
        Assert.Equal("div", output.TagName);
        Assert.Equal("edit-array-container", output.Attributes["class"].Value);
    }

    #endregion

    #region ProcessAsync - DisplayMode Validation Tests

    [Fact]
    public async Task ProcessAsync_WithDisplayModeWithoutDisplayViewName_ThrowsInvalidOperationException()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = null; // Invalid: DisplayMode requires DisplayViewName

        var context = CreateContext();
        var output = CreateOutput();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            tagHelper.ProcessAsync(context, output));

        Assert.Contains("DisplayViewName", exception.Message);
        Assert.Contains("required", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("DisplayMode", exception.Message);
    }

    [Fact]
    public async Task ProcessAsync_WithDisplayModeAndEmptyDisplayViewName_ThrowsInvalidOperationException()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = string.Empty; // Invalid: empty string

        var context = CreateContext();
        var output = CreateOutput();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            tagHelper.ProcessAsync(context, output));

        Assert.Contains("DisplayViewName", exception.Message);
        Assert.Contains("required", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcessAsync_WithDisplayModeAndWhitespaceDisplayViewName_ThrowsInvalidOperationException()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "   "; // Invalid: whitespace only

        var context = CreateContext();
        var output = CreateOutput();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            tagHelper.ProcessAsync(context, output));

        Assert.Contains("DisplayViewName", exception.Message);
        Assert.Contains("required", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcessAsync_WithDisplayModeFalseAndNullDisplayViewName_DoesNotThrow()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.DisplayMode = false;
        tagHelper.DisplayViewName = null; // Valid: DisplayViewName not needed when DisplayMode is false

        var context = CreateContext();
        var output = CreateOutput();

        // Act & Assert - Should complete without exception
        await tagHelper.ProcessAsync(context, output);

        // Verify it rendered successfully
        Assert.Equal("div", output.TagName);
        Assert.Equal("edit-array-container", output.Attributes["class"].Value.ToString());
    }

    [Fact]
    public async Task ProcessAsync_WithDisplayModeFalseAndEmptyDisplayViewName_DoesNotThrow()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.DisplayMode = false;
        tagHelper.DisplayViewName = ""; // Valid: DisplayViewName ignored when DisplayMode is false

        var context = CreateContext();
        var output = CreateOutput();

        // Act & Assert - Should complete without exception
        await tagHelper.ProcessAsync(context, output);

        Assert.Equal("div", output.TagName);
    }

    [Fact]
    public async Task ProcessAsync_WithDisplayModeAndValidDisplayViewName_RendersCorrectly()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "DisplayView"; // Valid configuration

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert - Should render display containers
        var content = GetOutputContent(output);
        Assert.Contains("display-container", content);
        Assert.Contains("edit-container", content);
    }

    [Fact]
    public async Task ProcessAsync_WithDisplayModeFalseAndSetDisplayViewName_IgnoresDisplayViewName()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.DisplayMode = false;
        tagHelper.DisplayViewName = "DisplayView"; // Set but ignored

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert - Should NOT render display containers (DisplayMode is false)
        var content = GetOutputContent(output);
        Assert.DoesNotContain("display-container", content);
    }

    [Fact]
    public async Task ProcessAsync_ValidationFailureMessage_IsDescriptive()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = null;

        var context = CreateContext();
        var output = CreateOutput();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            tagHelper.ProcessAsync(context, output));

        // Message should be helpful and guide the user to the solution
        Assert.Contains("DisplayViewName", exception.Message);
        Assert.Contains("DisplayMode", exception.Message);
        Assert.True(
            exception.Message.Contains("required") || exception.Message.Contains("must be specified"),
            "Exception message should indicate that DisplayViewName is required");
    }

    #endregion
    
    #region ProcessAsync - OnDelete Callback Tests
    
    [Fact]
    public async Task ProcessAsync_WithOnDeleteInDisplayMode_IncludesCallbackInDeleteButton()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "DisplayView";
        tagHelper.OnDelete = "myDeleteFunction";
        
        var context = CreateContext();
        var output = CreateOutput();
        
        // Act
        await tagHelper.ProcessAsync(context, output);
        
        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("onclick=\"markForDeletion('edit-array-test-item-0'); myDeleteFunction('edit-array-test-item-0');\"", content);
    }
    
    [Fact]
    public async Task ProcessAsync_WithOnDeleteInTemplate_IncludesCallbackInTemplateDeleteButton()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "DisplayView";
        tagHelper.RenderTemplate = true;
        tagHelper.OnDelete = "handleDelete";
        
        var context = CreateContext();
        var output = CreateOutput();
        
        // Act
        await tagHelper.ProcessAsync(context, output);
        
        // Assert
        var content = GetOutputContent(output);
        // Template should use this.closest pattern
        Assert.Contains("onclick=\"markForDeletion(this.closest('.edit-array-item').id); handleDelete(this.closest('.edit-array-item').id);\"", content);
    }
    
    [Fact]
    public async Task ProcessAsync_WithNullOnDelete_OnlyCallsMarkForDeletion()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "DisplayView";
        tagHelper.OnDelete = null;
        
        var context = CreateContext();
        var output = CreateOutput();
        
        // Act
        await tagHelper.ProcessAsync(context, output);
        
        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("onclick=\"markForDeletion('edit-array-test-item-0')\"", content);
        Assert.DoesNotContain("myDeleteFunction", content);
    }
    
    [Fact]
    public async Task ProcessAsync_WithEmptyOnDelete_OnlyCallsMarkForDeletion()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "DisplayView";
        tagHelper.OnDelete = "";
        
        var context = CreateContext();
        var output = CreateOutput();
        
        // Act
        await tagHelper.ProcessAsync(context, output);
        
        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("onclick=\"markForDeletion('edit-array-test-item-0')\"", content);
        Assert.DoesNotContain("(); ", content); // No empty callback invocation
    }
    
    [Fact]
    public async Task ProcessAsync_WithOnDeleteAndSpecialCharacters_EncodesCallback()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "DisplayView";
        tagHelper.OnDelete = "alert('deleted')"; // Contains quotes
        
        var context = CreateContext();
        var output = CreateOutput();
        
        // Act
        await tagHelper.ProcessAsync(context, output);
        
        // Assert
        var content = GetOutputContent(output);
        // Should encode the single quotes to prevent breaking HTML attributes
        // HtmlEncoder encodes single quotes as &#x27;
        Assert.Contains("alert(&#x27;deleted&#x27;)", content);
        // Should not contain unescaped quotes
        Assert.DoesNotContain("alert('deleted')", content);
    }
    
    [Fact]
    public async Task ProcessAsync_WithOnDeleteWithoutDisplayMode_DoesNotRenderDeleteButton()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.DisplayMode = false;
        tagHelper.OnDelete = "myDeleteFunction";
        
        var context = CreateContext();
        var output = CreateOutput();
        
        // Act
        await tagHelper.ProcessAsync(context, output);
        
        // Assert
        var content = GetOutputContent(output);
        Assert.DoesNotContain("delete-item-btn", content);
        Assert.DoesNotContain("myDeleteFunction", content);
    }
    
    [Fact]
    public async Task ProcessAsync_WithBothOnDeleteAndOnUpdate_IncludesBothCallbacks()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "DisplayView";
        tagHelper.OnDelete = "handleDelete";
        tagHelper.OnUpdate = "handleUpdate";
        
        var context = CreateContext();
        var output = CreateOutput();
        
        // Act
        await tagHelper.ProcessAsync(context, output);
        
        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("handleDelete", content);
        Assert.Contains("handleUpdate", content);
        Assert.Contains("markForDeletion", content);
    }
    
    [Fact]
    public async Task ProcessAsync_WithOnDeleteAndMultipleItems_AppliesCallbackToAllDeleteButtons()
    {
        // Arrange
        var items = new List<object> 
        { 
            new TestModel { Name = "Test1" },
            new TestModel { Name = "Test2" }
        };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "DisplayView";
        tagHelper.OnDelete = "deleteCallback";
        
        var context = CreateContext();
        var output = CreateOutput();
        
        // Act
        await tagHelper.ProcessAsync(context, output);
        
        // Assert
        var content = GetOutputContent(output);
        // Both items should have delete callback
        Assert.Contains("deleteCallback('edit-array-test-item-0')", content);
        Assert.Contains("deleteCallback('edit-array-test-item-1')", content);
    }
    
    #endregion

    #region ProcessAsync - Reordering Tests

    [Fact]
    public void EnableReordering_DefaultsToFalse()
    {
        // Arrange & Act
        var tagHelper = CreateTagHelper();

        // Assert
        Assert.False(tagHelper.EnableReordering);
    }

    [Fact]
    public async Task ProcessAsync_WithReorderingDisabled_DoesNotRenderReorderButtons()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "DisplayView";
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.DoesNotContain("reorder-btn", content);
        Assert.DoesNotContain("data-reorder-direction", content);
        Assert.False(output.Attributes.ContainsName("data-reorder-enabled"));
    }

    [Fact]
    public async Task ProcessAsync_WithReorderingEnabled_RendersReorderButtons()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.EnableReordering = true;
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "DisplayView";
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("true", output.Attributes["data-reorder-enabled"].Value);
        var content = GetOutputContent(output);
        Assert.Contains("data-reorder-direction=\"up\"", content);
        Assert.Contains("data-reorder-direction=\"down\"", content);
        Assert.Contains("moveItem('edit-array-test','edit-array-test-item-0',-1)", content);
        Assert.Contains("moveItem('edit-array-test','edit-array-test-item-0',1)", content);
    }

    [Fact]
    public async Task ProcessAsync_WithCustomReorderSettings_UsesCustomTextAndClass()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.EnableReordering = true;
        tagHelper.ReorderButtonCssClass = "theme-button";
        tagHelper.MoveUpButtonText = "Up!";
        tagHelper.MoveDownButtonText = "Down!";
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "DisplayView";
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("class=\"theme-button reorder-btn reorder-up-btn", content);
        Assert.Contains(">Up!</button>", content);
        Assert.Contains(">Down!</button>", content);
    }

    [Fact]
    public async Task ProcessAsync_WithTemplateAndReordering_RendersTemplateButtons()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.EnableReordering = true;
        tagHelper.RenderTemplate = true;
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "DisplayView";
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("template", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("data-reorder-direction=\"up\"", content);
        Assert.Contains("moveItem('edit-array-test', this.closest('.edit-array-item').id, -1)", content);
    }

    #endregion

    #region OnUpdate Encoding Tests

    [Fact]
    public async Task ProcessAsync_WithOnUpdateAndSpecialCharacters_EncodesCallback()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "DisplayView";
        tagHelper.OnUpdate = "alert('updated')"; // Contains quotes that need encoding

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        // HtmlEncoder encodes single quotes as &#x27;
        Assert.Contains("alert(&#x27;updated&#x27;)", content);
        // Should NOT contain unescaped quotes in the OnUpdate callback specifically
        Assert.DoesNotContain("alert('updated')", content);
    }

    [Fact]
    public async Task ProcessAsync_WithOnUpdateInTemplate_EncodesCallback()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "DisplayView";
        tagHelper.RenderTemplate = true;
        tagHelper.OnUpdate = "handleUpdate('special')"; // Contains quotes

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        // Check that template done button has encoded OnUpdate
        Assert.Contains("<template", content);
        Assert.Contains("handleUpdate(&#x27;special&#x27;)", content);
        // Should NOT contain unescaped callback
        Assert.DoesNotContain("handleUpdate('special')", content);
    }

    [Fact]
    public async Task ProcessAsync_WithOnUpdateContainingDoubleQuotes_EncodesProperly()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "DisplayView";
        tagHelper.OnUpdate = @"onUpdateCallback(param=""value"")"; // Contains escaped double quotes

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        // Verify the callback is encoded
        // HtmlEncoder encodes double quotes as &quot;
        Assert.Contains("onUpdateCallback(param=&quot;value&quot;)", content);
        // Should not have unescaped double quotes in onclick
        Assert.DoesNotContain(@"param=""value""", content);
    }

    [Fact]
    public async Task ProcessAsync_WithBothOnDeleteAndOnUpdateWithSpecialChars_EncodesConsistently()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "DisplayView";
        tagHelper.OnDelete = "handleDelete('test')";
        tagHelper.OnUpdate = "handleUpdate('test')";

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        // Both should be encoded identically
        Assert.Contains("handleDelete(&#x27;test&#x27;)", content);
        Assert.Contains("handleUpdate(&#x27;test&#x27;)", content);
        // Both should follow the same pattern for encoding
    }

    [Fact]
    public async Task ProcessAsync_WithOnUpdateInItemAndTemplateDisplayMode_EncodesBothInstances()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "DisplayView";
        tagHelper.RenderTemplate = true;
        tagHelper.OnUpdate = "myUpdate(\"data\")"; // Contains double quotes

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        // Should have two encoded instances: one in items, one in template
        var encodedCallback = "myUpdate(&quot;data&quot;)";
        var count = System.Text.RegularExpressions.Regex.Matches(content, System.Text.RegularExpressions.Regex.Escape(encodedCallback)).Count;
        Assert.Equal(2, count); // One in item done button, one in template done button
    }

    [Fact]
    public async Task ProcessAsync_WithEmptyOnUpdate_DoesNotIncludeCallback()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "DisplayView";
        tagHelper.OnUpdate = ""; // Empty callback

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        // Done button should only call toggleEditMode, not any callback
        var doneButtonMatch = System.Text.RegularExpressions.Regex.Matches(content, @"onclick=""toggleEditMode\('[^']+'\)""");
        Assert.NotEmpty(doneButtonMatch); // Should find done buttons with only toggleEditMode
    }

    [Fact]
    public async Task ProcessAsync_WithNullOnUpdate_DoesNotIncludeCallback()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "DisplayView";
        tagHelper.OnUpdate = null; // Null callback

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        // Done button should work fine with null OnUpdate
        Assert.Contains("done-edit-btn", content);
        Assert.DoesNotContain("null", content);
    }

    #endregion

    #region Id Validation and Encoding Tests

    [Fact]
    public async Task ProcessAsync_WithNullId_ThrowsInvalidOperationException()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.Id = null; // Explicitly set to null, overriding the helper default

        var context = CreateContext();
        var output = CreateOutput();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            tagHelper.ProcessAsync(context, output));
        Assert.Contains("id", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcessAsync_WithEmptyId_ThrowsInvalidOperationException()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.Id = ""; // Explicitly set to empty string

        var context = CreateContext();
        var output = CreateOutput();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            tagHelper.ProcessAsync(context, output));
        Assert.Contains("id", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcessAsync_WithWhitespaceOnlyId_ThrowsInvalidOperationException()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.Id = "   "; // Explicitly set to whitespace only

        var context = CreateContext();
        var output = CreateOutput();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            tagHelper.ProcessAsync(context, output));
        Assert.Contains("id", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcessAsync_WithSpecialCharactersInId_EncodesInHtmlAttribute()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        // ID with special characters that need encoding
        var tagHelper = CreateTagHelper(items: items, id: "test<script>");

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        // The id attribute should be HTML-encoded
        var idAttribute = output.Attributes["id"].Value;
        Assert.Contains("&lt;", idAttribute.ToString());
        Assert.Contains("&gt;", idAttribute.ToString());
        Assert.DoesNotContain("<script>", idAttribute.ToString());
    }

    [Fact]
    public async Task ProcessAsync_WithQuotesInId_EncodesProperly()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items, id: @"test""quote");

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        var idAttribute = output.Attributes["id"].Value.ToString();
        // Quote should be encoded in HTML attribute context
        Assert.Contains("&quot;", idAttribute);
        Assert.DoesNotContain("\"", idAttribute);
    }

    [Fact]
    public async Task ProcessAsync_WithIdContainingAmpersand_EncodesCorrectly()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items, id: "test&harmful");

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var idAttribute = output.Attributes["id"].Value.ToString();
        // Ampersand should be encoded
        Assert.Contains("&amp;", idAttribute);
    }

    [Fact]
    public async Task ProcessAsync_WithIdInJavaScriptContext_EncodesForJavascript()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items, id: "test'quote");
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "DisplayView";

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        // The ID used in onclick handlers should be properly escaped
        // For javascript context, single quotes need encoding
        Assert.DoesNotContain("toggleEditMode('test'quote')", content);
    }

    [Fact]
    public async Task ProcessAsync_WithValidId_UsesIdInAllLocations()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items, id: "myList");

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        var idAttribute = output.Attributes["id"].Value.ToString();
        // Verify ID attribute is set correctly with the encoded container ID
        Assert.Equal("edit-array-myList", idAttribute);
        // Verify ID appears in content (items container)
        Assert.Contains("edit-array-myList-items", content);
    }

    [Fact]
    public async Task ProcessAsync_WithIdInReorderButtons_EncodesForJavascript()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items, id: "list<xss>");
        tagHelper.EnableReordering = true;
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "DisplayView";

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        // ID in JavaScript should be properly escaped to prevent breaking the JS code
        // The escaped version should appear in moveItem calls
        Assert.Contains("moveItem", content);
        // Raw unencoded < and > should not appear in onclick
        Assert.DoesNotContain("moveItem('edit-array-list<xss>", content);
    }

    [Fact]
    public async Task ProcessAsync_WithIdContainingBackslash_EncodesAppropriately()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items, id: @"test\escape");

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        // Should handle backslash safely
        var idAttribute = output.Attributes["id"].Value.ToString();
        Assert.Contains("test", idAttribute);
    }

    #endregion

    #region CSS Class Encoding Tests

    [Fact]
    public async Task ProcessAsync_WithSpecialCharactersInContainerCssClass_EncodesClass()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.ContainerCssClass = "myClass<script>alert('xss')</script>";

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var classAttribute = output.Attributes["class"].Value.ToString();
        // Should encode < and >
        Assert.Contains("&lt;", classAttribute);
        Assert.Contains("&gt;", classAttribute);
        // Should NOT contain unencoded script tag
        Assert.DoesNotContain("<script>", classAttribute);
    }

    [Fact]
    public async Task ProcessAsync_WithQuotesInContainerCssClass_EncodesCorrectly()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.ContainerCssClass = @"myClass"" onclick=""alert('xss')""";

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var classAttribute = output.Attributes["class"].Value.ToString();
        // Quotes should be encoded
        Assert.Contains("&quot;", classAttribute);
        // Should NOT contain unencoded quotes that could break attribute
        Assert.DoesNotContain("\" onclick=", classAttribute);
    }

    [Fact]
    public async Task ProcessAsync_WithSpecialCharactersInItemCssClass_EncodesInHtml()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.ItemCssClass = "item<payload>";

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        // Should encode angle brackets
        Assert.Contains("&lt;", content);
        Assert.Contains("&gt;", content);
        // Should NOT contain unencoded brackets
        Assert.DoesNotContain("item<payload>", content);
    }

    [Fact]
    public async Task ProcessAsync_WithSpecialCharactersInButtonCssClass_EncodesInAllButtons()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "DisplayView";
        tagHelper.ButtonCssClass = "btn\"alert('xss')";

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        // Quotes should be encoded in button classes
        Assert.Contains("&quot;", content);
        // Should NOT contain unencoded quote that breaks attribute
        Assert.DoesNotContain("btn\"alert", content);
    }

    [Fact]
    public async Task ProcessAsync_WithAmpersandInCssClass_EncodesForHtml()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.ContainerCssClass = "class1&class2";

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var classAttribute = output.Attributes["class"].Value.ToString();
        // Ampersand should be encoded
        Assert.Contains("&amp;", classAttribute);
    }

    [Fact]
    public async Task ProcessAsync_WithReorderButtonsAndSpecialCharsInCssClass_EncodesReorderClass()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.EnableReordering = true;
        tagHelper.ReorderButtonCssClass = "btn<xss>";

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        // Reorder buttons should have encoded class
        Assert.Contains("&lt;xss&gt;", content);
        // Should NOT have unencoded brackets in button class
        Assert.DoesNotContain("btn<xss>", content);
    }

    [Fact]
    public async Task ProcessAsync_WithValidCssClasses_DoesNotAlterNormalClasses()
    {
        // Arrange - Test that normal CSS classes (without special chars) pass through encoding unchanged
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        // Set all CSS classes to normal values without special characters
        tagHelper.ContainerCssClass = "container-fluid";
        tagHelper.ItemCssClass = "list-item";
        tagHelper.ButtonCssClass = "btn btn-primary";
        tagHelper.ReorderButtonCssClass = "btn btn-outline-secondary";
        tagHelper.EnableReordering = true;

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);

        // Container class should appear unchanged in the container's class attribute
        var containerClass = output.Attributes["class"].Value.ToString();
        Assert.Equal("container-fluid", containerClass);

        // Item class should appear unchanged in the content
        Assert.Contains("list-item", content);

        // Reorder button class should appear unchanged in the reorder buttons
        Assert.Contains("btn btn-outline-secondary", content);

        // Normal classes should not be encoded (no HTML entities should appear for valid CSS names)
        // Check that the item class appears as-is, not encoded
        Assert.Contains("class=\"list-item\"", content);
    }

    [Fact]
    public async Task ProcessAsync_WithHtmlEntitiesInCssClass_EncodesAllChars()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.ItemCssClass = "class&nbsp;test";

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        // The & should be encoded to &amp;
        Assert.Contains("&amp;nbsp;", content);
    }

    [Fact]
    public async Task ProcessAsync_WithTemplateAndSpecialCharsInCssClass_EncodesTemplateClasses()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.RenderTemplate = true;
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "DisplayView";
        tagHelper.ItemCssClass = "template<test>";

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        // Template item class should also be encoded
        Assert.Contains("&lt;test&gt;", content);
        Assert.DoesNotContain("template<test>", content);
    }

    #endregion
}
