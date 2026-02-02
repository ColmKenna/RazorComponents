using System.Collections.Generic;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using RazorComponents.TagHelpers.Multiselect;
using Xunit;

namespace RazorComponents.Tests;

public class MultiselectCheckboxTagHelperTests : TagHelperTestBase<MultiselectCheckboxTagHelper>
{
    #region Test Models

    public class TestModel
    {
        public List<string> SelectedItems { get; set; } = new();
        public List<string> AvailableItems { get; set; } = new();
    }

    #endregion

    #region Helper Methods

    protected override MultiselectCheckboxTagHelper CreateTagHelper(Action<MultiselectCheckboxTagHelper>? configure = null)
    {
        var tagHelper = new MultiselectCheckboxTagHelper
        {
            ViewContext = CreateViewContext()
        };
        configure?.Invoke(tagHelper);
        return tagHelper;
    }

    private MultiselectCheckboxTagHelper CreateTagHelper(
        string title = "",
        string description = "",
        List<string>? availableItems = null,
        List<string>? selectedItems = null,
        string idPrefix = "checkbox",
        string fieldsetId = "")
    {
        var tagHelper = new MultiselectCheckboxTagHelper
        {
            Title = title,
            Description = description,
            IdPrefix = idPrefix,
            FieldsetId = fieldsetId,
            ViewContext = CreateViewContext()
        };

        if (availableItems != null)
        {
            tagHelper.Items = CreateItemsModelExpression(availableItems);
        }

        if (selectedItems != null)
        {
            tagHelper.For = CreateSelectedModelExpression(selectedItems);
        }

        return tagHelper;
    }

    private static ModelExpression CreateItemsModelExpression(List<string> items)
    {
        var metadataProvider = new EmptyModelMetadataProvider();
        var containerMetadata = metadataProvider.GetMetadataForType(typeof(TestModel));
        var modelExplorer = new ModelExplorer(metadataProvider, containerMetadata, new TestModel { AvailableItems = items })
            .GetExplorerForProperty("AvailableItems", items);
        return new ModelExpression("AvailableItems", modelExplorer);
    }

    private static ModelExpression CreateSelectedModelExpression(List<string> items)
    {
        var metadataProvider = new EmptyModelMetadataProvider();
        var containerMetadata = metadataProvider.GetMetadataForType(typeof(TestModel));
        var modelExplorer = new ModelExplorer(metadataProvider, containerMetadata, new TestModel { SelectedItems = items })
            .GetExplorerForProperty("SelectedItems", items);
        return new ModelExpression("SelectedItems", modelExplorer);
    }

    #endregion

    #region Happy Path Tests

    [Fact]
    public async Task ProcessAsync_WithValidInput_GeneratesExpectedOutput()
    {
        // Arrange
        var tagHelper = CreateTagHelper(
            title: "Select Items",
            description: "Choose your items",
            availableItems: new List<string> { "item1", "item2" },
            selectedItems: new List<string> { "item1" });
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("class=\"form-group\"", content);
        Assert.Contains("Select Items", content);
        Assert.Contains("Choose your items", content);
        Assert.Contains("type=\"checkbox\"", content);
    }

    [Fact]
    public async Task ProcessAsync_WithMultipleItems_GeneratesAllCheckboxes()
    {
        // Arrange
        var items = new List<string> { "apple", "banana", "cherry" };
        var tagHelper = CreateTagHelper(
            title: "Fruits",
            availableItems: items);
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("value=\"apple\"", content);
        Assert.Contains("value=\"banana\"", content);
        Assert.Contains("value=\"cherry\"", content);
    }

    [Fact]
    public async Task ProcessAsync_RemovesOriginalTag()
    {
        // Arrange
        var tagHelper = CreateTagHelper(title: "Test");
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Null(output.TagName);
    }

    #endregion

    #region Container Structure Tests

    [Fact]
    public async Task ProcessAsync_GeneratesOuterDivWithGroupClass()
    {
        // Arrange
        var tagHelper = CreateTagHelper(title: "Test");
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("<div class=\"form-group\">", content);
    }

    [Fact]
    public async Task ProcessAsync_GeneratesTitleLabel()
    {
        // Arrange
        var tagHelper = CreateTagHelper(title: "My Title");
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("<label class=\"form-label\">My Title</label>", content);
    }

    [Fact]
    public async Task ProcessAsync_GeneratesDescriptionDiv()
    {
        // Arrange
        var tagHelper = CreateTagHelper(title: "Test", description: "My Description");
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("<div class=\"form-text\">My Description</div>", content);
    }

    [Fact]
    public async Task ProcessAsync_GeneratesFieldset()
    {
        // Arrange
        var tagHelper = CreateTagHelper(title: "Test");
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("<fieldset class=\"multiselect-fieldset\"", content);
    }

    [Fact]
    public async Task ProcessAsync_WithFieldsetId_IncludesIdAttribute()
    {
        // Arrange
        var tagHelper = CreateTagHelper(title: "Test", fieldsetId: "my-fieldset");
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("id=\"my-fieldset\"", content);
    }

    [Fact]
    public async Task ProcessAsync_GeneratesGridContainer()
    {
        // Arrange
        var tagHelper = CreateTagHelper(
            title: "Test",
            availableItems: new List<string> { "item1" });
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("class=\"multiselect-grid\"", content);
        Assert.Contains("role=\"group\"", content);
    }

    [Fact]
    public async Task ProcessAsync_GeneratesOptionContainers()
    {
        // Arrange
        var tagHelper = CreateTagHelper(
            title: "Test",
            availableItems: new List<string> { "item1", "item2" });
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        // Should have two option containers
        var count = System.Text.RegularExpressions.Regex.Matches(content, "class=\"multiselect-option\"").Count;
        Assert.Equal(2, count);
    }

    #endregion

    #region Property Default Tests

    [Fact]
    public void Title_DefaultValue_IsEmpty()
    {
        var tagHelper = new MultiselectCheckboxTagHelper();
        Assert.Equal(string.Empty, tagHelper.Title);
    }

    [Fact]
    public void Description_DefaultValue_IsEmpty()
    {
        var tagHelper = new MultiselectCheckboxTagHelper();
        Assert.Equal(string.Empty, tagHelper.Description);
    }

    [Fact]
    public void EmptyMessage_DefaultValue_IsCorrect()
    {
        var tagHelper = new MultiselectCheckboxTagHelper();
        Assert.Equal("No items available. Contact your administrator.", tagHelper.EmptyMessage);
    }

    [Fact]
    public void GroupClass_DefaultValue_IsFormGroup()
    {
        var tagHelper = new MultiselectCheckboxTagHelper();
        Assert.Equal("form-group", tagHelper.GroupClass);
    }

    [Fact]
    public void LabelClass_DefaultValue_IsFormLabel()
    {
        var tagHelper = new MultiselectCheckboxTagHelper();
        Assert.Equal("form-label", tagHelper.LabelClass);
    }

    [Fact]
    public void DescriptionClass_DefaultValue_IsFormText()
    {
        var tagHelper = new MultiselectCheckboxTagHelper();
        Assert.Equal("form-text", tagHelper.DescriptionClass);
    }

    [Fact]
    public void FieldsetClass_DefaultValue_IsMultiselectFieldset()
    {
        var tagHelper = new MultiselectCheckboxTagHelper();
        Assert.Equal("multiselect-fieldset", tagHelper.FieldsetClass);
    }

    [Fact]
    public void GridClass_DefaultValue_IsMultiselectGrid()
    {
        var tagHelper = new MultiselectCheckboxTagHelper();
        Assert.Equal("multiselect-grid", tagHelper.GridClass);
    }

    [Fact]
    public void OptionClass_DefaultValue_IsMultiselectOption()
    {
        var tagHelper = new MultiselectCheckboxTagHelper();
        Assert.Equal("multiselect-option", tagHelper.OptionClass);
    }

    [Fact]
    public void InputClass_DefaultValue_IsMultiselectInput()
    {
        var tagHelper = new MultiselectCheckboxTagHelper();
        Assert.Equal("multiselect-input", tagHelper.InputClass);
    }

    [Fact]
    public void CheckboxLabelClass_DefaultValue_IsMultiselectLabel()
    {
        var tagHelper = new MultiselectCheckboxTagHelper();
        Assert.Equal("multiselect-label", tagHelper.CheckboxLabelClass);
    }

    [Fact]
    public void PillClass_DefaultValue_IsMultiselectPill()
    {
        var tagHelper = new MultiselectCheckboxTagHelper();
        Assert.Equal("multiselect-pill", tagHelper.PillClass);
    }

    [Fact]
    public void FieldsetId_DefaultValue_IsEmpty()
    {
        var tagHelper = new MultiselectCheckboxTagHelper();
        Assert.Equal(string.Empty, tagHelper.FieldsetId);
    }

    [Fact]
    public void IdPrefix_DefaultValue_IsCheckbox()
    {
        var tagHelper = new MultiselectCheckboxTagHelper();
        Assert.Equal("checkbox", tagHelper.IdPrefix);
    }

    [Fact]
    public void For_DefaultValue_IsNull()
    {
        var tagHelper = new MultiselectCheckboxTagHelper();
        Assert.Null(tagHelper.For);
    }

    [Fact]
    public void Items_DefaultValue_IsNull()
    {
        var tagHelper = new MultiselectCheckboxTagHelper();
        Assert.Null(tagHelper.Items);
    }

    #endregion

    #region Custom CSS Class Tests

    [Fact]
    public async Task ProcessAsync_WithCustomGroupClass_UsesCustomClass()
    {
        // Arrange
        var tagHelper = CreateTagHelper(title: "Test");
        tagHelper.GroupClass = "custom-group";
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("class=\"custom-group\"", content);
    }

    [Fact]
    public async Task ProcessAsync_WithCustomLabelClass_UsesCustomClass()
    {
        // Arrange
        var tagHelper = CreateTagHelper(title: "Test");
        tagHelper.LabelClass = "custom-label";
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("class=\"custom-label\"", content);
    }

    [Fact]
    public async Task ProcessAsync_WithCustomDescriptionClass_UsesCustomClass()
    {
        // Arrange
        var tagHelper = CreateTagHelper(title: "Test", description: "Desc");
        tagHelper.DescriptionClass = "custom-desc";
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("class=\"custom-desc\"", content);
    }

    [Fact]
    public async Task ProcessAsync_WithCustomFieldsetClass_UsesCustomClass()
    {
        // Arrange
        var tagHelper = CreateTagHelper(title: "Test");
        tagHelper.FieldsetClass = "custom-fieldset";
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("class=\"custom-fieldset\"", content);
    }

    [Fact]
    public async Task ProcessAsync_WithCustomGridClass_UsesCustomClass()
    {
        // Arrange
        var tagHelper = CreateTagHelper(
            title: "Test",
            availableItems: new List<string> { "item1" });
        tagHelper.GridClass = "custom-grid";
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("class=\"custom-grid\"", content);
    }

    [Fact]
    public async Task ProcessAsync_WithCustomOptionClass_UsesCustomClass()
    {
        // Arrange
        var tagHelper = CreateTagHelper(
            title: "Test",
            availableItems: new List<string> { "item1" });
        tagHelper.OptionClass = "custom-option";
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("class=\"custom-option\"", content);
    }

    [Fact]
    public async Task ProcessAsync_WithCustomInputClass_UsesCustomClass()
    {
        // Arrange
        var tagHelper = CreateTagHelper(
            title: "Test",
            availableItems: new List<string> { "item1" });
        tagHelper.InputClass = "custom-input";
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("class=\"custom-input\"", content);
    }

    [Fact]
    public async Task ProcessAsync_WithCustomCheckboxLabelClass_UsesCustomClass()
    {
        // Arrange
        var tagHelper = CreateTagHelper(
            title: "Test",
            availableItems: new List<string> { "item1" });
        tagHelper.CheckboxLabelClass = "custom-checkbox-label";
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("class=\"custom-checkbox-label\"", content);
    }

    [Fact]
    public async Task ProcessAsync_WithCustomPillClass_UsesCustomClass()
    {
        // Arrange
        var tagHelper = CreateTagHelper(
            title: "Test",
            availableItems: new List<string> { "item1" });
        tagHelper.PillClass = "custom-pill";
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("class=\"custom-pill\"", content);
    }

    #endregion

    #region Model Binding Tests

    [Fact]
    public async Task ProcessAsync_WithForExpression_UsesExpressionNameForInputName()
    {
        // Arrange
        var tagHelper = CreateTagHelper(
            title: "Test",
            availableItems: new List<string> { "item1" },
            selectedItems: new List<string>());
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("name=\"SelectedItems\"", content);
    }

    [Fact]
    public async Task ProcessAsync_WithItemsExpression_RendersAllItems()
    {
        // Arrange
        var items = new List<string> { "A", "B", "C" };
        var tagHelper = CreateTagHelper(
            title: "Test",
            availableItems: items);
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        foreach (var item in items)
        {
            Assert.Contains($"value=\"{item}\"", content);
        }
    }

    #endregion

    #region Checked State Tests

    [Fact]
    public async Task ProcessAsync_WithSelectedItem_CheckboxIsChecked()
    {
        // Arrange
        var tagHelper = CreateTagHelper(
            title: "Test",
            availableItems: new List<string> { "item1", "item2" },
            selectedItems: new List<string> { "item1" });
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        // item1 should be checked
        Assert.Contains("value=\"item1\"", content);
        Assert.Contains("checked=\"checked\"", content);
    }

    [Fact]
    public async Task ProcessAsync_WithNoSelectedItems_NoCheckboxesChecked()
    {
        // Arrange
        var tagHelper = CreateTagHelper(
            title: "Test",
            availableItems: new List<string> { "item1", "item2" },
            selectedItems: new List<string>());
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.DoesNotContain("checked=\"checked\"", content);
    }

    [Fact]
    public async Task ProcessAsync_WithAllItemsSelected_AllCheckboxesChecked()
    {
        // Arrange
        var items = new List<string> { "item1", "item2", "item3" };
        var tagHelper = CreateTagHelper(
            title: "Test",
            availableItems: items,
            selectedItems: items);
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        var checkedCount = System.Text.RegularExpressions.Regex.Matches(content, "checked=\"checked\"").Count;
        Assert.Equal(3, checkedCount);
    }

    [Fact]
    public async Task ProcessAsync_WithNullForExpression_NoCheckboxesChecked()
    {
        // Arrange
        var tagHelper = CreateTagHelper(
            title: "Test",
            availableItems: new List<string> { "item1", "item2" });
        // For is null by default
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.DoesNotContain("checked=\"checked\"", content);
    }

    #endregion

    #region ID Generation Tests

    [Fact]
    public async Task ProcessAsync_GeneratesUniqueIdsForCheckboxes()
    {
        // Arrange
        var tagHelper = CreateTagHelper(
            title: "Test",
            availableItems: new List<string> { "item1", "item2", "item3" },
            idPrefix: "checkbox");
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("id=\"checkbox-0\"", content);
        Assert.Contains("id=\"checkbox-1\"", content);
        Assert.Contains("id=\"checkbox-2\"", content);
    }

    [Fact]
    public async Task ProcessAsync_WithCustomIdPrefix_UsesPrefix()
    {
        // Arrange
        var tagHelper = CreateTagHelper(
            title: "Test",
            availableItems: new List<string> { "item1" },
            idPrefix: "scope-checkbox");
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("id=\"scope-checkbox-0\"", content);
    }

    [Fact]
    public async Task ProcessAsync_LabelForMatchesCheckboxId()
    {
        // Arrange
        var tagHelper = CreateTagHelper(
            title: "Test",
            availableItems: new List<string> { "item1" },
            idPrefix: "cb");
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("id=\"cb-0\"", content);
        Assert.Contains("for=\"cb-0\"", content);
    }

    #endregion

    #region ARIA Attribute Tests

    [Fact]
    public async Task ProcessAsync_GridHasRoleGroup()
    {
        // Arrange
        var tagHelper = CreateTagHelper(
            title: "Test",
            availableItems: new List<string> { "item1" });
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("role=\"group\"", content);
    }

    [Fact]
    public async Task ProcessAsync_GridHasAriaLabelledBy()
    {
        // Arrange
        var tagHelper = CreateTagHelper(
            title: "Test",
            availableItems: new List<string> { "item1" },
            fieldsetId: "my-fieldset");
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("aria-labelledby=\"my-fieldset-label\"", content);
    }

    [Fact]
    public async Task ProcessAsync_CheckboxHasAriaDescribedBy()
    {
        // Arrange
        var tagHelper = CreateTagHelper(
            title: "Test",
            availableItems: new List<string> { "item1" },
            idPrefix: "checkbox");
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("aria-describedby=\"checkbox-0-desc\"", content);
    }

    #endregion

    #region Empty State Tests

    [Fact]
    public async Task ProcessAsync_WithNullItems_ShowsEmptyMessage()
    {
        // Arrange
        var tagHelper = CreateTagHelper(title: "Test");
        // Items is null by default
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("No items available. Contact your administrator.", content);
    }

    [Fact]
    public async Task ProcessAsync_WithEmptyItems_ShowsEmptyMessage()
    {
        // Arrange
        var tagHelper = CreateTagHelper(
            title: "Test",
            availableItems: new List<string>());
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("No items available. Contact your administrator.", content);
    }

    [Fact]
    public async Task ProcessAsync_WithCustomEmptyMessage_ShowsCustomMessage()
    {
        // Arrange
        var tagHelper = CreateTagHelper(title: "Test");
        tagHelper.EmptyMessage = "Custom empty message";
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("Custom empty message", content);
    }

    [Fact]
    public async Task ProcessAsync_WithEmptyItems_DoesNotRenderGrid()
    {
        // Arrange
        var tagHelper = CreateTagHelper(
            title: "Test",
            availableItems: new List<string>());
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.DoesNotContain("class=\"multiselect-grid\"", content);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task ProcessAsync_WithEmptyTitle_OmitsTitleLabel()
    {
        // Arrange
        var tagHelper = CreateTagHelper(title: "");
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.DoesNotContain("<label class=\"form-label\"", content);
    }

    [Fact]
    public async Task ProcessAsync_WithEmptyDescription_OmitsDescriptionDiv()
    {
        // Arrange
        var tagHelper = CreateTagHelper(title: "Test", description: "");
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.DoesNotContain("<div class=\"form-text\"", content);
    }

    [Fact]
    public async Task ProcessAsync_WithEmptyFieldsetId_OmitsIdAttribute()
    {
        // Arrange
        var tagHelper = CreateTagHelper(title: "Test", fieldsetId: "");
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        // Should have fieldset but without id attribute
        Assert.Contains("<fieldset class=\"multiselect-fieldset\">", content);
    }

    [Fact]
    public async Task ProcessAsync_WithWhitespaceOnlyTitle_OmitsTitleLabel()
    {
        // Arrange
        var tagHelper = CreateTagHelper(title: "   ");
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.DoesNotContain("<label class=\"form-label\"", content);
    }

    [Fact]
    public async Task ProcessAsync_PillContainsItemLabel()
    {
        // Arrange
        var tagHelper = CreateTagHelper(
            title: "Test",
            availableItems: new List<string> { "myitem" });
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("<span class=\"multiselect-pill\">myitem</span>", content);
    }

    #endregion

    #region Security/XSS Prevention Tests

    [Fact]
    public async Task ProcessAsync_WithHtmlInTitle_EncodesTitle()
    {
        // Arrange
        var tagHelper = CreateTagHelper(title: "<script>alert('xss')</script>");
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.DoesNotContain("<script>", content);
        Assert.Contains("&lt;script&gt;", content);
    }

    [Fact]
    public async Task ProcessAsync_WithHtmlInDescription_EncodesDescription()
    {
        // Arrange
        var tagHelper = CreateTagHelper(title: "Test", description: "<img src=x onerror=alert('xss')>");
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.DoesNotContain("<img src=x", content);
        Assert.Contains("&lt;img", content);
    }

    [Fact]
    public async Task ProcessAsync_WithHtmlInItemLabel_EncodesLabel()
    {
        // Arrange
        var tagHelper = CreateTagHelper(
            title: "Test",
            availableItems: new List<string> { "<script>bad</script>" });
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.DoesNotContain("<script>bad</script>", content);
        Assert.Contains("&lt;script&gt;", content);
    }

    [Fact]
    public async Task ProcessAsync_WithHtmlInEmptyMessage_EncodesMessage()
    {
        // Arrange
        var tagHelper = CreateTagHelper(title: "Test");
        tagHelper.EmptyMessage = "<b>Bold</b> message";
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.DoesNotContain("<b>Bold</b>", content);
        Assert.Contains("&lt;b&gt;Bold&lt;/b&gt;", content);
    }

    [Theory]
    [InlineData("Test & More", "&amp;")]
    [InlineData("Test \"quoted\"", "&quot;")]
    [InlineData("Test 'single'", "&#x27;")]
    [InlineData("Test < Less", "&lt;")]
    [InlineData("Test > Greater", "&gt;")]
    public async Task ProcessAsync_WithSpecialCharactersInTitle_EncodesCorrectly(string title, string expectedEncoded)
    {
        // Arrange
        var tagHelper = CreateTagHelper(title: title);
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains(expectedEncoded, content);
    }

    #endregion

    #region HTML Structure Order Tests

    [Fact]
    public async Task ProcessAsync_OutputOrderIsCorrect()
    {
        // Arrange
        var tagHelper = CreateTagHelper(
            title: "Title",
            description: "Description",
            availableItems: new List<string> { "item1" });
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        var labelIndex = content.IndexOf("<label class=\"form-label\"", StringComparison.Ordinal);
        var descIndex = content.IndexOf("<div class=\"form-text\"", StringComparison.Ordinal);
        var fieldsetIndex = content.IndexOf("<fieldset", StringComparison.Ordinal);

        Assert.True(labelIndex < descIndex, "Label should come before description");
        Assert.True(descIndex < fieldsetIndex, "Description should come before fieldset");
    }

    [Fact]
    public async Task ProcessAsync_CheckboxComesBeforeLabel()
    {
        // Arrange
        var tagHelper = CreateTagHelper(
            title: "Test",
            availableItems: new List<string> { "item1" });
        var context = CreateContext();
        var output = CreateOutput("multiselect-checkbox");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        var inputIndex = content.IndexOf("<input", StringComparison.Ordinal);
        var labelIndex = content.IndexOf("<label class=\"multiselect-label\"", StringComparison.Ordinal);

        Assert.True(inputIndex < labelIndex, "Input should come before label");
    }

    #endregion
}
