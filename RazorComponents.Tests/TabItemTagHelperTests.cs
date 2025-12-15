using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using RazorComponents.TagHelpers.Tabs;
using Xunit;

namespace RazorComponents.Tests;

public class TabItemTagHelperTests : TagHelperTestBase<TabItemTagHelper>
{
    #region Helper Methods

    protected override TabItemTagHelper CreateTagHelper(Action<TabItemTagHelper>? configure = null)
    {
        var tagHelper = new TabItemTagHelper
        {
            Heading = "Tab Heading"
        };
        configure?.Invoke(tagHelper);
        return tagHelper;
    }

    private static TabItemTagHelper CreateTagHelper(
        string? id = null,
        bool selected = false,
        string? heading = null)
    {
        return new TabItemTagHelper
        {
            Id = id ?? string.Empty,
            Selected = selected,
            Heading = heading ?? "Tab Heading"
        };
    }

    private static new TagHelperContext CreateContext(
        string tagName = "tab-item",
        TagHelperAttributeList? attributes = null,
        string? parentTag = "tab")
    {
        var items = new Dictionary<object, object>();
        
        // Add parent tag info if specified
        if (parentTag != null)
        {
            items[typeof(TabTagHelper)] = new object();
        }

        return new TagHelperContext(
            tagName: tagName,
            allAttributes: attributes ?? new TagHelperAttributeList(),
            items: items,
            uniqueId: "test");
    }

    #endregion

    #region Happy Path Tests

    [Fact]
    public async Task ProcessAsync_WithValidInput_GeneratesExpectedOutput()
    {
        // Arrange
        var tagHelper = CreateTagHelper(id: "tab1", heading: "Home");
        var context = CreateContext();
        var output = CreateOutputWithContent("<p>Tab content</p>");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("<input class=\"tabs-panel-input\"", content);
        Assert.Contains("name=\"tabs\"", content);
        Assert.Contains("type=\"radio\"", content);
        Assert.Contains("id=\"tab1\"", content);
        Assert.Contains("<label class=\"tab-heading\"", content);
        Assert.Contains("for=\"tab1\"", content);
        Assert.Contains("Home", content);
        Assert.Contains("<p>Tab content</p>", content);
    }

    [Fact]
    public async Task ProcessAsync_WithAllProperties_GeneratesCompleteOutput()
    {
        // Arrange
        var tagHelper = CreateTagHelper(id: "my-tab", selected: true, heading: "Complete Tab");
        var context = CreateContext();
        var output = CreateOutputWithContent("<span>Rich content</span>");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("id=\"my-tab\"", content);
        Assert.Contains("checked=\"checked\"", content);
        Assert.Contains("Complete Tab", content);
        Assert.Contains("<span>Rich content</span>", content);
    }

    [Fact]
    public async Task ProcessAsync_RemovesTabItemTag()
    {
        // Arrange
        var tagHelper = CreateTagHelper(heading: "Test");
        var context = CreateContext();
        var output = CreateOutputWithContent("content");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Null(output.TagName);
    }

    #endregion

    #region Property Tests

    [Fact]
    public async Task ProcessAsync_WithProvidedId_UsesProvidedId()
    {
        // Arrange
        var tagHelper = CreateTagHelper(id: "custom-id", heading: "Tab");
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("id=\"custom-id\"", content);
        Assert.Contains("for=\"custom-id\"", content);
    }

    [Fact]
    public async Task ProcessAsync_WithEmptyId_GeneratesIdFromHeading()
    {
        // Arrange
        var tagHelper = CreateTagHelper(id: "", heading: "My Tab");
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        // ID should be generated from heading: "my-tab"
        Assert.Contains("id=\"my-tab\"", content);
        Assert.Contains("for=\"my-tab\"", content);
    }

    [Fact]
    public async Task ProcessAsync_WithNullId_GeneratesIdFromHeading()
    {
        // Arrange
        var tagHelper = CreateTagHelper(id: null, heading: "Test Heading");
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        // ID should be generated from heading
        Assert.Contains("id=\"test-heading\"", content);
    }

    [Fact]
    public async Task ProcessAsync_WithSelectedTrue_AddsCheckedAttribute()
    {
        // Arrange
        var tagHelper = CreateTagHelper(id: "tab1", selected: true, heading: "Tab");
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("checked=\"checked\"", content);
    }

    [Fact]
    public async Task ProcessAsync_WithSelectedFalse_NoCheckedAttribute()
    {
        // Arrange
        var tagHelper = CreateTagHelper(id: "tab1", selected: false, heading: "Tab");
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.DoesNotContain("checked=\"checked\"", content);
    }

    [Fact]
    public async Task ProcessAsync_WithHeading_IncludesHeadingInLabel()
    {
        // Arrange
        var tagHelper = CreateTagHelper(heading: "My Section");
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("<label class=\"tab-heading\"", content);
        Assert.Contains("My Section</label>", content);
    }

    [Fact]
    public async Task ProcessAsync_WithDifferentHeadings_GeneratesDifferentIds()
    {
        // Arrange
        var tagHelper1 = CreateTagHelper(id: "", heading: "First Tab");
        var tagHelper2 = CreateTagHelper(id: "", heading: "Second Tab");
        
        var context1 = CreateContext();
        var output1 = CreateOutputWithContent("");
        
        var context2 = CreateContext();
        var output2 = CreateOutputWithContent("");

        // Act
        await tagHelper1.ProcessAsync(context1, output1);
        await tagHelper2.ProcessAsync(context2, output2);

        // Assert
        var content1 = GetOutputContent(output1);
        var content2 = GetOutputContent(output2);
        
        Assert.Contains("id=\"first-tab\"", content1);
        Assert.Contains("id=\"second-tab\"", content2);
    }

    #endregion

    #region ID Generation Tests

    [Theory]
    [InlineData("Simple Tab", "simple-tab")]
    [InlineData("Tab With Multiple Words", "tab-with-multiple-words")]
    [InlineData("UPPERCASE TAB", "uppercase-tab")]
    [InlineData("MixedCase Tab", "mixedcase-tab")]
    public async Task ProcessAsync_WithVariousHeadings_GeneratesCorrectIds(
        string heading, string expectedId)
    {
        // Arrange
        var tagHelper = CreateTagHelper(id: "", heading: heading);
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains($"id=\"{expectedId}\"", content);
    }

    [Theory]
    [InlineData("Tab With Numbers 123")]
    [InlineData("Tab-With-Hyphens")]
    [InlineData("tab_with_underscores")]
    public async Task ProcessAsync_WithSpecialCharactersInHeading_RemovesInvalidChars(
        string heading)
    {
        // Arrange
        var tagHelper = CreateTagHelper(id: "", heading: heading);
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        // Should not contain the original special characters in the ID
        Assert.Contains("id=\"", content);
        Assert.DoesNotContain("_", content.Split("id=\"")[1].Split("\"")[0]);
    }

    [Fact]
    public async Task ProcessAsync_WithHeadingContainingPunctuation_RemovesPunctuation()
    {
        // Arrange
        var tagHelper = CreateTagHelper(id: "", heading: "Tab! @#$%^&*()");
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        var idMatch = System.Text.RegularExpressions.Regex.Match(content, "id=\"([^\"]+)\"");
        Assert.True(idMatch.Success);
        var idValue = idMatch.Groups[1].Value;
        // Should only contain alphanumeric characters and hyphens
        Assert.Matches(@"^[a-z0-9-]+$", idValue);
    }

    [Fact]
    public async Task ProcessAsync_WithEmptyHeading_GeneratesValidId()
    {
        // Arrange
        var tagHelper = CreateTagHelper(id: "", heading: "");
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("id=\"\"", content);
    }

    [Fact]
    public async Task ProcessAsync_WithWhitespaceOnlyHeading_GeneratesValidId()
    {
        // Arrange
        var tagHelper = CreateTagHelper(id: "", heading: "   ");
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        // Should have an id attribute, though it may be empty or contain hyphens from spaces
        Assert.Contains("id=\"", content);
    }

    #endregion

    #region HTML Structure Tests

    [Fact]
    public async Task ProcessAsync_GeneratesProperInputStructure()
    {
        // Arrange
        var tagHelper = CreateTagHelper(id: "test-tab", heading: "Test");
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        var inputMatch = System.Text.RegularExpressions.Regex.Match(
            content, 
            "<input[^>]*class=\"tabs-panel-input\"[^>]*>");
        
        Assert.True(inputMatch.Success);
        var inputTag = inputMatch.Value;
        
        Assert.Contains("class=\"tabs-panel-input\"", inputTag);
        Assert.Contains("name=\"tabs\"", inputTag);
        Assert.Contains("type=\"radio\"", inputTag);
        Assert.Contains("id=\"test-tab\"", inputTag);
    }

    [Fact]
    public async Task ProcessAsync_GeneratesProperLabelStructure()
    {
        // Arrange
        var tagHelper = CreateTagHelper(id: "test-tab", heading: "My Tab");
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("<label class=\"tab-heading\" for=\"test-tab\">My Tab</label>", content);
    }

    [Fact]
    public async Task ProcessAsync_GeneratesProperPanelStructure()
    {
        // Arrange
        var tabContent = "<span>Panel content</span>";
        var tagHelper = CreateTagHelper(heading: "Tab");
        var context = CreateContext();
        var output = CreateOutputWithContent(tabContent);

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("<div class=\"panel\">", content);
        Assert.Contains("<div class=\"panel-content\">", content);
        Assert.Contains(tabContent, content);
        Assert.Contains("</div></div>", content);
    }

    [Fact]
    public async Task ProcessAsync_PreservesChildContent()
    {
        // Arrange
        var childContent = "<div><h2>Heading</h2><p>Paragraph</p><ul><li>Item</li></ul></div>";
        var tagHelper = CreateTagHelper(heading: "Tab");
        var context = CreateContext();
        var output = CreateOutputWithContent(childContent);

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("<h2>Heading</h2>", content);
        Assert.Contains("<p>Paragraph</p>", content);
        Assert.Contains("<li>Item</li>", content);
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [Fact]
    public async Task ProcessAsync_WithVeryLongHeading_ProcessesSuccessfully()
    {
        // Arrange
        var longHeading = string.Concat(Enumerable.Repeat("Word ", 100));
        var tagHelper = CreateTagHelper(id: "", heading: longHeading);
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("id=\"", content);
        Assert.Contains(longHeading, content);
    }

    [Fact]
    public async Task ProcessAsync_WithVeryLongId_UsesProvidedId()
    {
        // Arrange
        var longId = string.Concat(Enumerable.Repeat("tab-", 50));
        var tagHelper = CreateTagHelper(id: longId, heading: "Tab");
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains($"id=\"{longId}\"", content);
    }

    [Fact]
    public async Task ProcessAsync_WithHtmlInChildContent_PreservesHtml()
    {
        // Arrange
        var htmlContent = "<strong>Bold</strong> and <em>italic</em>";
        var tagHelper = CreateTagHelper(heading: "Tab");
        var context = CreateContext();
        var output = CreateOutputWithContent(htmlContent);

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("<strong>Bold</strong>", content);
        Assert.Contains("<em>italic</em>", content);
    }

    [Fact]
    public async Task ProcessAsync_WithEmptyChildContent_StillGeneratesStructure()
    {
        // Arrange
        var tagHelper = CreateTagHelper(id: "empty-tab", heading: "Empty");
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("id=\"empty-tab\"", content);
        Assert.Contains("Empty", content);
        Assert.Contains("<div class=\"panel\">", content);
    }

    [Fact]
    public async Task ProcessAsync_WithWhitespaceInHeading_PreservesWhitespace()
    {
        // Arrange
        var tagHelper = CreateTagHelper(heading: "  Multi   Word  Heading  ");
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("  Multi   Word  Heading  ", content);
    }

    #endregion

    #region Context Tests

    [Fact]
    public async Task ProcessAsync_WithParentContext_ProcessesSuccessfully()
    {
        // Arrange
        var tagHelper = CreateTagHelper(heading: "Tab");
        var context = CreateContext(parentTag: "tab");
        var output = CreateOutputWithContent("");

        // Act & Assert - Should not throw
        await tagHelper.ProcessAsync(context, output);
    }

    [Fact]
    public async Task ProcessAsync_WithoutParentContext_StillProcesses()
    {
        // Arrange
        var tagHelper = CreateTagHelper(heading: "Tab");
        var context = CreateContext(parentTag: null);
        var output = CreateOutputWithContent("");

        // Act & Assert - Should not throw
        await tagHelper.ProcessAsync(context, output);
    }

    #endregion

    #region Multiple Tabs Tests

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task ProcessAsync_WithVariousTabIndices_GeneratesUniqueIds(int tabCount)
    {
        // Arrange
        var outputs = new List<TagHelperOutput>();
        var ids = new HashSet<string>();

        for (int i = 1; i <= tabCount; i++)
        {
            var tagHelper = CreateTagHelper(id: "", heading: $"Tab {i}");
            var context = CreateContext();
            var output = CreateOutputWithContent("");
            
            // Act
            await tagHelper.ProcessAsync(context, output);
            
            // Assert
            var content = GetOutputContent(output);
            var idMatch = System.Text.RegularExpressions.Regex.Match(content, "id=\"([^\"]+)\"");
            Assert.True(idMatch.Success);
            var id = idMatch.Groups[1].Value;
            ids.Add(id);
        }

        // Verify all IDs are unique
        Assert.Equal(tabCount, ids.Count);
    }

    #endregion

    #region Attribute Tests

    [Fact]
    public async Task ProcessAsync_InputHasCorrectName()
    {
        // Arrange
        var tagHelper = CreateTagHelper(heading: "Tab");
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("name=\"tabs\"", content);
    }

    [Fact]
    public async Task ProcessAsync_InputHasCorrectType()
    {
        // Arrange
        var tagHelper = CreateTagHelper(heading: "Tab");
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("type=\"radio\"", content);
    }

    [Fact]
    public async Task ProcessAsync_InputHasCorrectClass()
    {
        // Arrange
        var tagHelper = CreateTagHelper(heading: "Tab");
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("class=\"tabs-panel-input\"", content);
    }

    [Fact]
    public async Task ProcessAsync_LabelHasCorrectClass()
    {
        // Arrange
        var tagHelper = CreateTagHelper(heading: "Tab");
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("class=\"tab-heading\"", content);
    }

    [Fact]
    public async Task ProcessAsync_PanelHasCorrectClasses()
    {
        // Arrange
        var tagHelper = CreateTagHelper(heading: "Tab");
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("class=\"panel\"", content);
        Assert.Contains("class=\"panel-content\"", content);
    }

    #endregion

    #region Output Order Tests

    [Fact]
    public async Task ProcessAsync_OutputOrderIsCorrect()
    {
        // Arrange
        var tagHelper = CreateTagHelper(id: "tab1", heading: "Test Tab");
        var context = CreateContext();
        var output = CreateOutputWithContent("Content");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        var inputIndex = content.IndexOf("<input");
        var labelIndex = content.IndexOf("<label");
        var panelIndex = content.IndexOf("<div class=\"panel\"");

        // Input should come before label which should come before panel
        Assert.True(inputIndex < labelIndex && labelIndex < panelIndex);
    }

    #endregion

    #region Special Characters Tests

    [Fact]
    public async Task ProcessAsync_WithHtmlEntityInHeading_EncodesEntity()
    {
        // Arrange
        var tagHelper = CreateTagHelper(heading: "Tab &amp; More");
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        // HTML entity should be double-encoded to prevent XSS
        Assert.Contains("Tab &amp;amp; More", content);
    }

    [Fact]
    public async Task ProcessAsync_WithQuotesInHeading_EncodesQuotes()
    {
        // Arrange
        var tagHelper = CreateTagHelper(heading: "Tab \"quoted\"");
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        // Quotes should be encoded to prevent attribute injection
        Assert.Contains("Tab &quot;quoted&quot;", content);
    }

    #endregion

    #region Security Tests (XSS Prevention)

    [Fact]
    public async Task Process_WithMaliciousHeading_EncodesOutput()
    {
        // Arrange
        var tagHelper = CreateTagHelper(heading: "<script>alert('xss')</script>");
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = TagHelperTestBase<TabItemTagHelper>.GetOutputContent(output);
        // Should contain encoded version &lt;script&gt;...
        Assert.False(content.Contains("<script>", StringComparison.Ordinal));
        Assert.True(content.Contains("&lt;script&gt;", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("<img src=x onerror=alert('xss')>", "&lt;img")]
    [InlineData("<svg onload=alert('xss')>", "&lt;svg")]
    [InlineData("javascript:alert('xss')", "javascript:alert")]
    [InlineData("<iframe src='javascript:alert(1)'>", "&lt;iframe")]
    public async Task Process_WithVariousMaliciousInputs_EncodesOutput(string maliciousInput, string expectedEncoded)
    {
        // Arrange
        var tagHelper = CreateTagHelper(heading: maliciousInput);
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = TagHelperTestBase<TabItemTagHelper>.GetOutputContent(output);
        Assert.True(content.Contains(expectedEncoded, StringComparison.Ordinal));
    }

    #endregion

    #region TabContext Integration Tests

    [Fact]
    public async Task TabItem_RegistersSelf_WithParentContext()
    {
        // Arrange
        var tagHelper = CreateTagHelper(id: "test-tab", heading: "Test");

        // Create a context with TabContext already set (simulating parent TabTagHelper)
        var context = CreateContext();
        var tabContext = new TabContext();
        context.Items[typeof(TabContext)] = tabContext;

        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal(1, tabContext.Tabs.Count);
        Assert.Equal("test-tab", tabContext.Tabs[0].Id);
        Assert.False(tabContext.Tabs[0].IsSelected);
    }

    [Fact]
    public async Task TabItem_WithSelected_RegistersAsSelected()
    {
        // Arrange
        var tagHelper = CreateTagHelper(id: "selected-tab", selected: true, heading: "Selected");

        var context = CreateContext();
        var tabContext = new TabContext();
        context.Items[typeof(TabContext)] = tabContext;

        var output = CreateOutputWithContent("");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal(1, tabContext.Tabs.Count);
        Assert.True(tabContext.Tabs[0].IsSelected);
    }

    [Fact]
    public async Task TabItem_WithoutParentContext_StillWorks()
    {
        // Arrange
        var tagHelper = CreateTagHelper(id: "standalone-tab", heading: "Standalone");
        var context = CreateContext();
        var output = CreateOutputWithContent("");

        // Act & Assert - Should not throw
        await tagHelper.ProcessAsync(context, output);

        // Should still generate valid output
        var content = TagHelperTestBase<TabItemTagHelper>.GetOutputContent(output);
        Assert.Contains("id=\"standalone-tab\"", content);
    }

    #endregion
}
