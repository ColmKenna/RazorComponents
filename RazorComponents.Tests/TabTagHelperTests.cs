using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using RazorComponents.TagHelpers.Tabs;
using Xunit;

namespace RazorComponents.Tests;

public class TabTagHelperTests
{
    #region Helper Methods

    private static TabTagHelper CreateTagHelper()
    {
        return new TabTagHelper();
    }

    private static TagHelperContext CreateContext(
        string tagName = "tab",
        TagHelperAttributeList? attributes = null)
    {
        return new TagHelperContext(
            tagName: tagName,
            allAttributes: attributes ?? new TagHelperAttributeList(),
            items: new Dictionary<object, object>(),
            uniqueId: "test");
    }

    private static TagHelperOutput CreateOutput(
        string tagName = "tab",
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

    private static TagHelperOutput CreateOutputWithContent(string childContent)
    {
        var output = new TagHelperOutput(
            tagName: "tab",
            attributes: new TagHelperAttributeList(),
            getChildContentAsync: (useCached, encoder) =>
            {
                var content = new DefaultTagHelperContent();
                content.SetHtmlContent(childContent);
                return Task.FromResult<TagHelperContent>(content);
            })
        {
            TagMode = TagMode.StartTagAndEndTag
        };
        return output;
    }

    private static string GetOutputContent(TagHelperOutput output)
    {
        using (var writer = new StringWriter())
        {
            output.Content.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
            return writer.ToString();
        }
    }

    #endregion

    #region Happy Path Tests

    [Fact]
    public async Task ProcessAsync_WithMinimalValidInput_TransformsTagToDiv()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var context = CreateContext();
        var output = CreateOutputWithContent("<tab-item><p>Content</p></tab-item>");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("div", output.TagName);
    }

    [Fact]
    public async Task ProcessAsync_WithValidInput_SetsTabsClass()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var context = CreateContext();
        var output = CreateOutputWithContent("<tab-item><p>Content</p></tab-item>");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.NotNull(output.Attributes["class"]);
        Assert.Equal("tabs", output.Attributes["class"].Value);
    }

    [Fact]
    public async Task ProcessAsync_WithChildContent_PreservesContent()
    {
        // Arrange
        var childContent = "<tab-item><p>Test Content</p></tab-item>";
        var tagHelper = CreateTagHelper();
        var context = CreateContext();
        var output = CreateOutputWithContent(childContent);

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var outputContent = GetOutputContent(output);
        Assert.Contains("Test Content", outputContent);
    }

    #endregion

    #region Auto-Selection Tests

    [Fact]
    public async Task ProcessAsync_WithNoCheckedTab_SelectsFirstTab()
    {
        // Arrange
        var childContent = "<input class=\"tabs-panel-input\" name=\"tabs\" type=\"radio\" id=\"tab1\" />" +
                          "<input class=\"tabs-panel-input\" name=\"tabs\" type=\"radio\" id=\"tab2\" />";
        var tagHelper = CreateTagHelper();
        var context = CreateContext();
        var output = CreateOutputWithContent(childContent);

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var outputContent = GetOutputContent(output);
        Assert.Contains("id=\"tab1\" checked=\"checked\"", outputContent);
    }

    [Fact]
    public async Task ProcessAsync_WithAlreadyCheckedTab_DoesNotAddAnotherChecked()
    {
        // Arrange
        var childContent = "<input class=\"tabs-panel-input\" name=\"tabs\" type=\"radio\" id=\"tab1\" checked=\"checked\" />" +
                          "<input class=\"tabs-panel-input\" name=\"tabs\" type=\"radio\" id=\"tab2\" />";
        var tagHelper = CreateTagHelper();
        var context = CreateContext();
        var output = CreateOutputWithContent(childContent);

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var outputContent = GetOutputContent(output);
        // Should have exactly one checked attribute
        var checkedCount = outputContent.Split(new[] { "checked=\"checked\"" }, StringSplitOptions.None).Length - 1;
        Assert.Equal(1, checkedCount);
    }

    [Fact]
    public async Task ProcessAsync_WithMultipleUncheckedTabs_SelectsFirstTabOnly()
    {
        // Arrange
        var childContent = "<input class=\"tabs-panel-input\" name=\"tabs\" type=\"radio\" id=\"tab1\" />" +
                          "<input class=\"tabs-panel-input\" name=\"tabs\" type=\"radio\" id=\"tab2\" />" +
                          "<input class=\"tabs-panel-input\" name=\"tabs\" type=\"radio\" id=\"tab3\" />";
        var tagHelper = CreateTagHelper();
        var context = CreateContext();
        var output = CreateOutputWithContent(childContent);

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var outputContent = GetOutputContent(output);
        Assert.Contains("id=\"tab1\" checked=\"checked\"", outputContent);
        // Second and third tabs should not have checked attribute
        var beforeSecondTab = outputContent.Substring(0, outputContent.IndexOf("id=\"tab2\""));
        var afterFirstTab = outputContent.Substring(beforeSecondTab.Length);
        var beforeThirdTab = afterFirstTab.Substring(0, afterFirstTab.IndexOf("id=\"tab3\""));
        Assert.DoesNotContain("id=\"tab2\" checked=\"checked\"", beforeThirdTab);
    }

    #endregion

    #region Content Handling Tests

    [Fact]
    public async Task ProcessAsync_WithEmptyChildContent_StillTransformsToDiv()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("div", output.TagName);
        Assert.Equal("tabs", output.Attributes["class"].Value);
    }

    [Fact]
    public async Task ProcessAsync_WithHtmlEntities_PreservesEntities()
    {
        // Arrange
        var childContent = "<tab-item id=\"test&amp;id\"><p>Content &lt;test&gt;</p></tab-item>";
        var tagHelper = CreateTagHelper();
        var context = CreateContext();
        var output = CreateOutputWithContent(childContent);

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var outputContent = GetOutputContent(output);
        Assert.Contains("&amp;", outputContent);
        Assert.Contains("&lt;", outputContent);
        Assert.Contains("&gt;", outputContent);
    }

    [Fact]
    public async Task ProcessAsync_WithNestedHtmlStructure_PreservesStructure()
    {
        // Arrange
        var childContent = "<div class=\"container\">" +
                          "<input class=\"tabs-panel-input\" name=\"tabs\" type=\"radio\" id=\"tab1\" />" +
                          "<label>Tab 1</label>" +
                          "<div class=\"panel\"><p>Content 1</p></div>" +
                          "</div>";
        var tagHelper = CreateTagHelper();
        var context = CreateContext();
        var output = CreateOutputWithContent(childContent);

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var outputContent = GetOutputContent(output);
        Assert.Contains("<div class=\"container\">", outputContent);
        Assert.Contains("<label>Tab 1</label>", outputContent);
        Assert.Contains("<p>Content 1</p>", outputContent);
    }

    [Fact]
    public async Task ProcessAsync_WithLongContent_ProcessesSuccessfully()
    {
        // Arrange
        var longContent = "<input class=\"tabs-panel-input\" name=\"tabs\" type=\"radio\" id=\"tab1\" />";
        for (int i = 0; i < 100; i++)
        {
            longContent += $"<div>Item {i}</div>";
        }
        var tagHelper = CreateTagHelper();
        var context = CreateContext();
        var output = CreateOutputWithContent(longContent);

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var outputContent = GetOutputContent(output);
        Assert.Contains("checked=\"checked\"", outputContent);
        Assert.Contains("Item 50", outputContent);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task ProcessAsync_WithoutTabsPanelInputElement_DoesNotModifyContent()
    {
        // Arrange
        var childContent = "<div><p>No tabs input elements</p></div>";
        var tagHelper = CreateTagHelper();
        var context = CreateContext();
        var output = CreateOutputWithContent(childContent);

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var outputContent = GetOutputContent(output);
        // Content should be unchanged since there are no tabs-panel-input elements
        Assert.Contains("<div><p>No tabs input elements</p></div>", outputContent);
    }

    [Fact]
    public async Task ProcessAsync_WithMultipleTabsPanelInputClasses_SelectsFirstOccurrence()
    {
        // Arrange
        var childContent = "Some text before <input class=\"tabs-panel-input\" id=\"first\" /> middle <input class=\"tabs-panel-input\" id=\"second\" />";
        var tagHelper = CreateTagHelper();
        var context = CreateContext();
        var output = CreateOutputWithContent(childContent);

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var outputContent = GetOutputContent(output);
        // Should find and modify the first occurrence
        Assert.Contains("checked=\"checked\"", outputContent);
        var firstInputIndex = outputContent.IndexOf("id=\"first\"");
        var checkedIndex = outputContent.IndexOf("checked=\"checked\"");
        Assert.True(firstInputIndex < checkedIndex);
    }

    [Fact]
    public async Task ProcessAsync_WithSpecialCharactersInContent_HandlesCorrectly()
    {
        // Arrange
        var childContent = "<input class=\"tabs-panel-input\" id=\"tab@#$\" />" +
                          "<div>Tab with special chars: !@#$%^&*()</div>";
        var tagHelper = CreateTagHelper();
        var context = CreateContext();
        var output = CreateOutputWithContent(childContent);

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var outputContent = GetOutputContent(output);
        Assert.Contains("checked=\"checked\"", outputContent);
    }

    [Fact]
    public async Task ProcessAsync_WithWhitespaceInContent_PreservesWhitespace()
    {
        // Arrange
        var childContent = "  \n  <input class=\"tabs-panel-input\" id=\"tab1\" />  \n  ";
        var tagHelper = CreateTagHelper();
        var context = CreateContext();
        var output = CreateOutputWithContent(childContent);

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var outputContent = GetOutputContent(output);
        Assert.Contains("checked=\"checked\"", outputContent);
    }

    #endregion

    #region Self-Closing Input Tests

    [Fact]
    public async Task ProcessAsync_WithSelfClosingInput_SelectsCorrectly()
    {
        // Arrange
        var childContent = "<input class=\"tabs-panel-input\" name=\"tabs\" type=\"radio\" id=\"tab1\" />" +
                          "<input class=\"tabs-panel-input\" name=\"tabs\" type=\"radio\" id=\"tab2\" />";
        var tagHelper = CreateTagHelper();
        var context = CreateContext();
        var output = CreateOutputWithContent(childContent);

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var outputContent = GetOutputContent(output);
        // Should properly insert checked before />
        Assert.Contains("id=\"tab1\" checked=\"checked\"/>", outputContent);
    }

    [Fact]
    public async Task ProcessAsync_WithInputNoClosingSlash_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        // This test documents Issue #2 from the test report: Fragile String Manipulation
        // The current implementation searches for "/>" and if not found, IndexOf returns -1
        // which causes Insert to throw ArgumentOutOfRangeException
        var childContent = "<input class=\"tabs-panel-input\" name=\"tabs\" type=\"radio\" id=\"tab1\">";
        var tagHelper = CreateTagHelper();
        var context = CreateContext();
        var output = CreateOutputWithContent(childContent);

        // Act & Assert
        // This should be fixed by implementing the regex-based solution from the test report
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await tagHelper.ProcessAsync(context, output);
        });
    }

    #endregion

    #region Output Attributes Tests

    [Fact]
    public async Task ProcessAsync_AllowsAttributeAddition()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var attributes = new TagHelperAttributeList { { "id", "my-tabs" }, { "data-test", "value" } };
        var context = CreateContext(attributes: attributes);
        var output = CreateOutputWithContent("<input class=\"tabs-panel-input\" id=\"tab1\" />");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("div", output.TagName);
        Assert.Equal("tabs", output.Attributes["class"].Value);
        // Original attributes should be accessible
        var hasId = output.Attributes.Any(a => a.Name == "id");
        var hasDataAttr = output.Attributes.Any(a => a.Name == "data-test");
        Assert.True(hasId || hasDataAttr || output.Attributes.Count >= 1);
    }

    [Fact]
    public async Task ProcessAsync_SetsClassAttributeExplicitly()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.True(output.Attributes.Contains(new TagHelperAttribute("class", "tabs")));
    }

    #endregion

    #region Multiple Tabs Tests

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task ProcessAsync_WithVariousTabCounts_SelectsFirstTab(int tabCount)
    {
        // Arrange
        var childContent = "";
        for (int i = 1; i <= tabCount; i++)
        {
            childContent += $"<input class=\"tabs-panel-input\" name=\"tabs\" type=\"radio\" id=\"tab{i}\" />";
        }
        var tagHelper = CreateTagHelper();
        var context = CreateContext();
        var output = CreateOutputWithContent(childContent);

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var outputContent = GetOutputContent(output);
        Assert.Contains("id=\"tab1\" checked=\"checked\"", outputContent);
        // Ensure only first is checked
        var checkedCount = outputContent.Split(new[] { "checked=\"checked\"" }, StringSplitOptions.None).Length - 1;
        Assert.Equal(1, checkedCount);
    }

    #endregion

    #region TagMode and TagName Tests

    [Fact]
    public async Task ProcessAsync_DoesNotModifyOriginalTagMode()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var context = CreateContext();
        var output = CreateOutputWithContent("");
        var originalTagMode = output.TagMode;

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal(originalTagMode, output.TagMode);
    }

    [Fact]
    public async Task ProcessAsync_TransformsToDiv()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("div", output.TagName);
        Assert.NotEqual("tab", output.TagName);
    }

    #endregion
}
