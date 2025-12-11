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
    #region ProcessAsync - Custom CSS Classes Tests

    [Fact]
    public async Task ProcessAsync_WithCustomContainerClass_UsesCustomClass()
    {
        // Arrange
        var tagHelper = CreateTagHelper(items: new List<object>());
        tagHelper.ContainerCssClass = "my-custom-container";
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("my-custom-container", output.Attributes["class"].Value);
    }

    [Fact]
    public async Task ProcessAsync_WithDefaultContainerClass_UsesDefaultClass()
    {
        // Arrange
        var tagHelper = CreateTagHelper(items: new List<object>());
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("edit-array-container", output.Attributes["class"].Value);
    }

    [Fact]
    public async Task ProcessAsync_WithCustomItemClass_AppliesClassToItems()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test1" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.ItemCssClass = "my-custom-item";
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains($"<div class=\"{tagHelper.ItemCssClass}\"", content);
    }

    [Fact]
    public async Task ProcessAsync_WithDefaultItemClass_AppliesDefaultClassToItems()
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
    }

    [Fact]
    public async Task ProcessAsync_WithCustomButtonClass_AppliesCustomClassToButtons()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test1" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "DisplayView";
        tagHelper.ButtonCssClass = "my-btn-class";
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains($"class=\"{tagHelper.ButtonCssClass}", content);
    }

    [Fact]
    public async Task ProcessAsync_WithCustomButtonClassAndTemplate_AppliesCustomClassToTemplateButtons()
    {
        // Arrange
        var items = new List<object> { new TestModel { Name = "Test1" } };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.RenderTemplate = true;
        tagHelper.ShowAddButton = true;
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "DisplayView";
        tagHelper.ButtonCssClass = "custom-btn";
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        // Should contain buttons with custom class in template
        Assert.Contains("template", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("custom-btn", content);
    }

    [Fact]
    public async Task ProcessAsync_WithMultipleCustomClasses_AppliesAllCorrectly()
    {
        // Arrange
        var items = new List<object> 
        { 
            new TestModel { Name = "Test1" },
            new TestModel { Name = "Test2" }
        };
        var tagHelper = CreateTagHelper(items: items);
        tagHelper.ContainerCssClass = "custom-container";
        tagHelper.ItemCssClass = "custom-item";
        tagHelper.ButtonCssClass = "custom-btn";
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("custom-container", output.Attributes["class"].Value);
        var content = GetOutputContent(output);
        Assert.Contains("<div class=\"custom-item\"", content);
        // Verify all items use custom class (2 items)
        var itemCount = content.Split($"<div class=\"custom-item\"").Length - 1;
        Assert.Equal(2, itemCount);
    }

    #endregion
}
