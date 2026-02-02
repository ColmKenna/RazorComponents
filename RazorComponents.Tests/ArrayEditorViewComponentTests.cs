using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using RazorComponents.Pages.Shared.Components.ArrayEditor;
using Xunit;

namespace RazorComponents.Tests;

/// <summary>
/// Comprehensive test suite for ArrayEditorViewComponent.
/// Tests cover happy paths, edge cases, parameter variations, and model validation.
/// </summary>
public class ArrayEditorViewComponentTests
{
    #region Helper Methods

    /// <summary>
    /// Creates a new instance of ArrayEditorViewComponent for testing.
    /// </summary>
    private static ArrayEditorViewComponent CreateComponent()
    {
        return new ArrayEditorViewComponent();
    }

    #endregion

    #region Happy Path Tests

    [Fact]
    public void Invoke_WithValidInputs_ReturnsViewWithModel()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1", "Item2", "Item3" };

        // Act
        var result = component.Invoke("myArray", items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        Assert.NotNull(viewResult.ViewData);
        Assert.NotNull(viewResult.ViewData.Model);
    }

    [Fact]
    public void Invoke_WithAllParametersProvided_ReturnsViewWithCompleteModel()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Value1", "Value2" };

        // Act
        var result = component.Invoke(
            id: "customId",
            items: items,
            cssClass: "custom-class",
            name: "CustomName",
            deleteName: "CustomDeleteName",
            title: "Custom Title");

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.NotNull(model);
        Assert.Equal("customId", model.Id);
        Assert.Equal(2, model.Items.Count);
        Assert.Equal("custom-class", model.CssClass);
        Assert.Equal("CustomName", model.Name);
        Assert.Equal("CustomDeleteName", model.DeleteName);
        Assert.Equal("Custom Title", model.Title);
    }

    [Fact]
    public void Invoke_WithMinimalParameters_ReturnsViewWithDefaultValues()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };

        // Act
        var result = component.Invoke("testId", items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal("testId", model.Id);
        Assert.Single(model.Items);
        Assert.Equal("", model.CssClass);
        Assert.Equal("Items", model.Name);
        Assert.Equal("DeletedItems", model.DeleteName);
        Assert.Equal("", model.Title);
    }

    [Fact]
    public void Invoke_WithMultipleItems_PreservesItemOrder()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "First", "Second", "Third", "Fourth" };

        // Act
        var result = component.Invoke("orderedArray", items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal(4, model.Items.Count);
        Assert.Equal("First", model.Items[0]);
        Assert.Equal("Second", model.Items[1]);
        Assert.Equal("Third", model.Items[2]);
        Assert.Equal("Fourth", model.Items[3]);
    }

    #endregion

    #region Parameter Tests

    [Fact]
    public void Invoke_WithCustomId_AssignsIdToModel()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };

        // Act
        var result = component.Invoke("custom-id-123", items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal("custom-id-123", model.Id);
    }

    [Fact]
    public void Invoke_WithCustomCssClass_AssignsCssClassToModel()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };

        // Act
        var result = component.Invoke("arrayId", items, cssClass: "my-custom-class another-class");

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal("my-custom-class another-class", model.CssClass);
    }

    [Fact]
    public void Invoke_WithCustomName_AssignsNameToModel()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };

        // Act
        var result = component.Invoke("arrayId", items, name: "MyCustomItems");

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal("MyCustomItems", model.Name);
    }

    [Fact]
    public void Invoke_WithCustomDeleteName_AssignsDeleteNameToModel()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };

        // Act
        var result = component.Invoke("arrayId", items, deleteName: "CustomDeletedItems");

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal("CustomDeletedItems", model.DeleteName);
    }

    [Fact]
    public void Invoke_WithCustomTitle_AssignsTitleToModel()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };

        // Act
        var result = component.Invoke("arrayId", items, title: "My Array Editor");

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal("My Array Editor", model.Title);
    }

    [Fact]
    public void Invoke_WithDefaultCssClass_AssignsEmptyStringToModel()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };

        // Act
        var result = component.Invoke("arrayId", items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal("", model.CssClass);
    }

    [Fact]
    public void Invoke_WithDefaultName_AssignsItemsToModel()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };

        // Act
        var result = component.Invoke("arrayId", items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal("Items", model.Name);
    }

    [Fact]
    public void Invoke_WithDefaultDeleteName_AssignsDeletedItemsToModel()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };

        // Act
        var result = component.Invoke("arrayId", items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal("DeletedItems", model.DeleteName);
    }

    [Fact]
    public void Invoke_WithDefaultTitle_AssignsEmptyStringToModel()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };

        // Act
        var result = component.Invoke("arrayId", items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal("", model.Title);
    }

    #endregion

    #region Edge Case Tests - Empty and Null Values

    [Fact]
    public void Invoke_WithEmptyItems_ReturnsViewWithEmptyList()
    {
        // Arrange
        var component = CreateComponent();
        var items = Array.Empty<string>();

        // Act
        var result = component.Invoke("arrayId", items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.NotNull(model.Items);
        Assert.Empty(model.Items);
    }

    [Fact]
    public void Invoke_WithNullItems_ThrowsArgumentNullException()
    {
        // Arrange
        var component = CreateComponent();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => component.Invoke("arrayId", null));
        Assert.Equal("source", exception.ParamName);
    }

    [Fact]
    public void Invoke_WithEmptyStringId_AssignsEmptyStringToModel()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };

        // Act
        var result = component.Invoke("", items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal("", model.Id);
    }

    [Fact]
    public void Invoke_WithWhitespaceOnlyId_AssignsWhitespaceToModel()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };

        // Act
        var result = component.Invoke("   ", items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal("   ", model.Id);
    }

    [Fact]
    public void Invoke_WithEmptyStringCssClass_AssignsEmptyStringToModel()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };

        // Act
        var result = component.Invoke("arrayId", items, cssClass: "");

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal("", model.CssClass);
    }

    [Fact]
    public void Invoke_WithWhitespaceOnlyCssClass_AssignsWhitespaceToModel()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };

        // Act
        var result = component.Invoke("arrayId", items, cssClass: "   ");

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal("   ", model.CssClass);
    }

    [Fact]
    public void Invoke_WithEmptyStringName_AssignsEmptyStringToModel()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };

        // Act
        var result = component.Invoke("arrayId", items, name: "");

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal("", model.Name);
    }

    [Fact]
    public void Invoke_WithEmptyStringDeleteName_AssignsEmptyStringToModel()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };

        // Act
        var result = component.Invoke("arrayId", items, deleteName: "");

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal("", model.DeleteName);
    }

    [Fact]
    public void Invoke_WithEmptyStringTitle_AssignsEmptyStringToModel()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };

        // Act
        var result = component.Invoke("arrayId", items, title: "");

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal("", model.Title);
    }

    #endregion

    #region Edge Case Tests - Special Characters

    [Fact]
    public void Invoke_WithSpecialCharactersInId_AssignsSpecialCharactersToModel()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };

        // Act
        var result = component.Invoke("array-id_123.test", items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal("array-id_123.test", model.Id);
    }

    [Fact]
    public void Invoke_WithHtmlInId_AssignsHtmlToModel()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };

        // Act
        var result = component.Invoke("<script>alert('xss')</script>", items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal("<script>alert('xss')</script>", model.Id);
        // Note: Encoding should happen at render time in the view, not in the component
    }

    [Fact]
    public void Invoke_WithQuotesInTitle_AssignsQuotesToModel()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };

        // Act
        var result = component.Invoke("arrayId", items, title: "The \"Best\" Array");

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal("The \"Best\" Array", model.Title);
    }

    [Fact]
    public void Invoke_WithUnicodeCharactersInItems_PreservesUnicode()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Hello 世界", "Emoji 😀", "Ñoño" };

        // Act
        var result = component.Invoke("arrayId", items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal(3, model.Items.Count);
        Assert.Equal("Hello 世界", model.Items[0]);
        Assert.Equal("Emoji 😀", model.Items[1]);
        Assert.Equal("Ñoño", model.Items[2]);
    }

    [Fact]
    public void Invoke_WithHtmlInItems_PreservesHtml()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "<b>Bold</b>", "<script>alert('test')</script>", "&lt;encoded&gt;" };

        // Act
        var result = component.Invoke("arrayId", items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal(3, model.Items.Count);
        Assert.Equal("<b>Bold</b>", model.Items[0]);
        Assert.Equal("<script>alert('test')</script>", model.Items[1]);
        Assert.Equal("&lt;encoded&gt;", model.Items[2]);
    }

    #endregion

    #region Edge Case Tests - Very Long Strings

    [Fact]
    public void Invoke_WithVeryLongId_HandlesCorrectly()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };
        var longId = new string('a', 10000);

        // Act
        var result = component.Invoke(longId, items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal(10000, model.Id.Length);
        Assert.Equal(longId, model.Id);
    }

    [Fact]
    public void Invoke_WithVeryLongTitle_HandlesCorrectly()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };
        var longTitle = new string('T', 5000);

        // Act
        var result = component.Invoke("arrayId", items, title: longTitle);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal(5000, model.Title.Length);
        Assert.Equal(longTitle, model.Title);
    }

    [Fact]
    public void Invoke_WithVeryLongItemValue_HandlesCorrectly()
    {
        // Arrange
        var component = CreateComponent();
        var longValue = new string('x', 10000);
        var items = new[] { longValue };

        // Act
        var result = component.Invoke("arrayId", items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Single(model.Items);
        Assert.Equal(10000, model.Items[0].Length);
        Assert.Equal(longValue, model.Items[0]);
    }

    #endregion

    #region Edge Case Tests - Collections

    [Fact]
    public void Invoke_WithSingleItem_ReturnsSingleItemList()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "OnlyItem" };

        // Act
        var result = component.Invoke("arrayId", items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Single(model.Items);
        Assert.Equal("OnlyItem", model.Items[0]);
    }

    [Fact]
    public void Invoke_WithManyItems_HandlesLargeCollections()
    {
        // Arrange
        var component = CreateComponent();
        var items = Enumerable.Range(1, 1000).Select(i => $"Item{i}").ToArray();

        // Act
        var result = component.Invoke("arrayId", items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal(1000, model.Items.Count);
        Assert.Equal("Item1", model.Items[0]);
        Assert.Equal("Item1000", model.Items[999]);
    }

    [Fact]
    public void Invoke_WithEmptyStringItems_PreservesEmptyStrings()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "", "Valid", "", "AlsoValid", "" };

        // Act
        var result = component.Invoke("arrayId", items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal(5, model.Items.Count);
        Assert.Equal("", model.Items[0]);
        Assert.Equal("Valid", model.Items[1]);
        Assert.Equal("", model.Items[2]);
        Assert.Equal("AlsoValid", model.Items[3]);
        Assert.Equal("", model.Items[4]);
    }

    [Fact]
    public void Invoke_WithWhitespaceOnlyItems_PreservesWhitespace()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "   ", "Valid", "\t", "AlsoValid", " " };

        // Act
        var result = component.Invoke("arrayId", items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal(5, model.Items.Count);
        Assert.Equal("   ", model.Items[0]);
        Assert.Equal("Valid", model.Items[1]);
        Assert.Equal("\t", model.Items[2]);
        Assert.Equal("AlsoValid", model.Items[3]);
        Assert.Equal(" ", model.Items[4]);
    }

    [Fact]
    public void Invoke_WithDuplicateItems_PreservesDuplicates()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1", "Item2", "Item1", "Item2", "Item1" };

        // Act
        var result = component.Invoke("arrayId", items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.Equal(5, model.Items.Count);
        Assert.Equal("Item1", model.Items[0]);
        Assert.Equal("Item2", model.Items[1]);
        Assert.Equal("Item1", model.Items[2]);
        Assert.Equal("Item2", model.Items[3]);
        Assert.Equal("Item1", model.Items[4]);
    }

    #endregion

    #region Model Structure Tests

    [Fact]
    public void Invoke_ReturnsCorrectModelType()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };

        // Act
        var result = component.Invoke("arrayId", items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData.Model);
    }

    [Fact]
    public void Invoke_ModelContainsExpectedData()
    {
        // Arrange
        var component = CreateComponent();
        var expectedItems = new[] { "First", "Second", "Third" };

        // Act
        var result = component.Invoke("testId", expectedItems, "test-class", "TestName", "TestDeleteName", "Test Title");

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);

        Assert.Equal("testId", model.Id);
        Assert.Equal(3, model.Items.Count);
        Assert.Equal("First", model.Items[0]);
        Assert.Equal("Second", model.Items[1]);
        Assert.Equal("Third", model.Items[2]);
        Assert.Equal("test-class", model.CssClass);
        Assert.Equal("TestName", model.Name);
        Assert.Equal("TestDeleteName", model.DeleteName);
        Assert.Equal("Test Title", model.Title);
    }

    [Fact]
    public void Invoke_ModelDeletedItemsListIsInitializedButEmpty()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };

        // Act
        var result = component.Invoke("arrayId", items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);
        Assert.NotNull(model.DeletedItems);
        Assert.Empty(model.DeletedItems);
    }

    [Fact]
    public void Invoke_ModelItemsIsNewList()
    {
        // Arrange
        var component = CreateComponent();
        var originalItems = new[] { "Item1", "Item2" };

        // Act
        var result = component.Invoke("arrayId", originalItems);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);

        // Verify it's a new list (not the same reference)
        Assert.NotSame(originalItems, model.Items);
        // But contains the same values
        Assert.Equal(originalItems.Length, model.Items.Count);
        Assert.Equal(originalItems[0], model.Items[0]);
        Assert.Equal(originalItems[1], model.Items[1]);
    }

    #endregion

    #region Return Type Tests

    [Fact]
    public void Invoke_ReturnsViewViewComponentResult()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };

        // Act
        var result = component.Invoke("arrayId", items);

        // Assert
        Assert.IsType<ViewViewComponentResult>(result);
    }

    [Fact]
    public void Invoke_ReturnsDefaultView()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };

        // Act
        var result = component.Invoke("arrayId", items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        // Null ViewName means convention-based "Default" view
        Assert.Null(viewResult.ViewName);
    }

    [Fact]
    public void Invoke_ViewDataContainsModel()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Item1" };

        // Act
        var result = component.Invoke("arrayId", items);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        Assert.NotNull(viewResult.ViewData);
        Assert.NotNull(viewResult.ViewData.Model);
    }

    #endregion

    #region Integration Tests - Complex Scenarios

    [Fact]
    public void Invoke_WithComplexRealWorldScenario_HandlesCorrectly()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[]
        {
            "john.doe@example.com",
            "jane.smith@example.com",
            "admin@company.co.uk",
            "",
            "user+tag@domain.com"
        };

        // Act
        var result = component.Invoke(
            id: "email-list-editor",
            items: items,
            cssClass: "form-control array-editor email-list",
            name: "EmailAddresses",
            deleteName: "RemovedEmails",
            title: "Email Recipients");

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);

        Assert.Equal("email-list-editor", model.Id);
        Assert.Equal(5, model.Items.Count);
        Assert.Equal("john.doe@example.com", model.Items[0]);
        Assert.Equal("", model.Items[3]);
        Assert.Equal("user+tag@domain.com", model.Items[4]);
        Assert.Equal("form-control array-editor email-list", model.CssClass);
        Assert.Equal("EmailAddresses", model.Name);
        Assert.Equal("RemovedEmails", model.DeleteName);
        Assert.Equal("Email Recipients", model.Title);
        Assert.Empty(model.DeletedItems);
    }

    [Fact]
    public void Invoke_WithNestedFormScenario_GeneratesCorrectFieldNames()
    {
        // Arrange
        var component = CreateComponent();
        var items = new[] { "Skill1", "Skill2", "Skill3" };

        // Act
        var result = component.Invoke(
            id: "user-skills",
            items: items,
            name: "User.Profile.Skills",
            deleteName: "User.Profile.DeletedSkills");

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<ArrayEditorViewComponent.ArrayEditorModel>(viewResult.ViewData!.Model);

        Assert.Equal("User.Profile.Skills", model.Name);
        Assert.Equal("User.Profile.DeletedSkills", model.DeleteName);
        Assert.Equal(3, model.Items.Count);
    }

    #endregion
}
