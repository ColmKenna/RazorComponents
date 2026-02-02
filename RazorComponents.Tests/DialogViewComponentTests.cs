using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using RazorComponents.Pages.Shared.Components.Dialog;
using Xunit;

namespace RazorComponents.Tests;

/// <summary>
/// Comprehensive test suite for DialogViewComponent.
/// Tests cover happy paths, edge cases, parameter variations, model population, and architectural concerns.
/// </summary>
public class DialogViewComponentTests
{
    #region Helper Methods

    /// <summary>
    /// Creates a new instance of DialogViewComponent for testing.
    /// </summary>
    private static DialogViewComponent CreateComponent()
    {
        return new DialogViewComponent();
    }

    #endregion

    #region Happy Path Tests

    [Fact]
    public void Invoke_WithValidInputs_ReturnsViewWithComponent()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[]
        {
            new DialogInputModel { Id = "input1", Type = "text" },
            new DialogInputModel { Id = "input2", Type = "email" }
        };

        // Act
        var result = component.Invoke("myDialog", "Test Title", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        Assert.NotNull(viewResult.ViewData);
        Assert.NotNull(viewResult.ViewData.Model);
    }

    [Fact]
    public void Invoke_WithAllParametersProvided_PopulatesAllProperties()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[]
        {
            new DialogInputModel { Id = "name", Type = "text" },
            new DialogInputModel { Id = "email", Type = "email" }
        };

        // Act
        var result = component.Invoke("customDialog", "Custom Dialog Title", inputs, "Submit Now");

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Equal("customDialog", model.DialogId);
        Assert.Equal("Custom Dialog Title", model.Title);
        Assert.Equal(2, model.Inputs.Count());
        Assert.Equal("Submit Now", model.ButtonText);
    }

    [Fact]
    public void Invoke_WithMinimalParameters_UsesDefaultButtonText()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };

        // Act
        var result = component.Invoke("dialog1", "My Dialog", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Equal("dialog1", model.DialogId);
        Assert.Equal("My Dialog", model.Title);
        Assert.Single(model.Inputs);
        Assert.Equal("Open Dialog", model.ButtonText);
    }

    [Fact]
    public void Invoke_WithMultipleInputs_PreservesInputOrder()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[]
        {
            new DialogInputModel { Id = "first", Type = "text" },
            new DialogInputModel { Id = "second", Type = "email" },
            new DialogInputModel { Id = "third", Type = "password" },
            new DialogInputModel { Id = "fourth", Type = "number" }
        };

        // Act
        var result = component.Invoke("orderedDialog", "Form", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        var inputList = model.Inputs.ToList();
        Assert.Equal(4, inputList.Count);
        Assert.Equal("first", inputList[0].Id);
        Assert.Equal("second", inputList[1].Id);
        Assert.Equal("third", inputList[2].Id);
        Assert.Equal("fourth", inputList[3].Id);
    }

    #endregion

    #region Parameter Tests

    [Fact]
    public void Invoke_WithCustomDialogId_AssignsDialogIdToProperty()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };

        // Act
        var result = component.Invoke("custom-dialog-id-123", "Title", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Equal("custom-dialog-id-123", model.DialogId);
    }

    [Fact]
    public void Invoke_WithCustomTitle_AssignsTitleToProperty()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };

        // Act
        var result = component.Invoke("dialogId", "My Custom Title", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Equal("My Custom Title", model.Title);
    }

    [Fact]
    public void Invoke_WithCustomInputs_AssignsInputsToProperty()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[]
        {
            new DialogInputModel { Id = "username", Type = "text" },
            new DialogInputModel { Id = "password", Type = "password" }
        };

        // Act
        var result = component.Invoke("dialogId", "Title", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Equal(2, model.Inputs.Count());
        Assert.Equal("username", model.Inputs.First().Id);
        Assert.Equal("password", model.Inputs.Last().Id);
    }

    [Fact]
    public void Invoke_WithCustomButtonText_AssignsButtonTextToProperty()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };

        // Act
        var result = component.Invoke("dialogId", "Title", inputs, "Custom Button");

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Equal("Custom Button", model.ButtonText);
    }

    [Fact]
    public void Invoke_WithDefaultButtonText_AssignsOpenDialogToProperty()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };

        // Act
        var result = component.Invoke("dialogId", "Title", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Equal("Open Dialog", model.ButtonText);
    }

    #endregion

    #region Edge Case Tests - Empty and Null Values

    [Fact]
    public void Invoke_WithEmptyInputs_AssignsEmptyCollectionToProperty()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = Array.Empty<DialogInputModel>();

        // Act
        var result = component.Invoke("dialogId", "Title", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.NotNull(model.Inputs);
        Assert.Empty(model.Inputs);
    }

    [Fact]
    public void Invoke_WithNullInputs_AssignsNullToProperty()
    {
        // Arrange
        var component = CreateComponent();

        // Act
        var result = component.Invoke("dialogId", "Title", null!);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Null(model.Inputs);
    }

    [Fact]
    public void Invoke_WithEmptyStringDialogId_AssignsEmptyStringToProperty()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };

        // Act
        var result = component.Invoke("", "Title", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Equal("", model.DialogId);
    }

    [Fact]
    public void Invoke_WithWhitespaceOnlyDialogId_AssignsWhitespaceToProperty()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };

        // Act
        var result = component.Invoke("   ", "Title", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Equal("   ", model.DialogId);
    }

    [Fact]
    public void Invoke_WithEmptyStringTitle_AssignsEmptyStringToProperty()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };

        // Act
        var result = component.Invoke("dialogId", "", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Equal("", model.Title);
    }

    [Fact]
    public void Invoke_WithWhitespaceOnlyTitle_AssignsWhitespaceToProperty()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };

        // Act
        var result = component.Invoke("dialogId", "   ", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Equal("   ", model.Title);
    }

    [Fact]
    public void Invoke_WithEmptyStringButtonText_AssignsEmptyStringToProperty()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };

        // Act
        var result = component.Invoke("dialogId", "Title", inputs, "");

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Equal("", model.ButtonText);
    }

    [Fact]
    public void Invoke_WithWhitespaceOnlyButtonText_AssignsWhitespaceToProperty()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };

        // Act
        var result = component.Invoke("dialogId", "Title", inputs, "   ");

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Equal("   ", model.ButtonText);
    }

    #endregion

    #region Edge Case Tests - Special Characters

    [Fact]
    public void Invoke_WithSpecialCharactersInDialogId_AssignsSpecialCharactersToProperty()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };

        // Act
        var result = component.Invoke("dialog-id_123.test", "Title", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Equal("dialog-id_123.test", model.DialogId);
    }

    [Fact]
    public void Invoke_WithHtmlInDialogId_AssignsHtmlToProperty()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };

        // Act
        var result = component.Invoke("<script>alert('xss')</script>", "Title", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Equal("<script>alert('xss')</script>", model.DialogId);
        // Note: Encoding should happen at render time in the view, not in the component
    }

    [Fact]
    public void Invoke_WithQuotesInTitle_AssignsQuotesToProperty()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };

        // Act
        var result = component.Invoke("dialogId", "The \"Best\" Dialog", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Equal("The \"Best\" Dialog", model.Title);
    }

    [Fact]
    public void Invoke_WithUnicodeCharactersInTitle_PreservesUnicode()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };

        // Act
        var result = component.Invoke("dialogId", "Hello 世界 😀 Ñoño", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Equal("Hello 世界 😀 Ñoño", model.Title);
    }

    [Fact]
    public void Invoke_WithHtmlInTitle_PreservesHtml()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };

        // Act
        var result = component.Invoke("dialogId", "<b>Bold</b> Title", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Equal("<b>Bold</b> Title", model.Title);
    }

    [Fact]
    public void Invoke_WithSpecialCharactersInButtonText_PreservesCharacters()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };

        // Act
        var result = component.Invoke("dialogId", "Title", inputs, "Click & Submit!");

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Equal("Click & Submit!", model.ButtonText);
    }

    #endregion

    #region Edge Case Tests - Very Long Strings

    [Fact]
    public void Invoke_WithVeryLongDialogId_HandlesCorrectly()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };
        var longId = new string('a', 10000);

        // Act
        var result = component.Invoke(longId, "Title", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Equal(10000, model.DialogId.Length);
        Assert.Equal(longId, model.DialogId);
    }

    [Fact]
    public void Invoke_WithVeryLongTitle_HandlesCorrectly()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };
        var longTitle = new string('T', 5000);

        // Act
        var result = component.Invoke("dialogId", longTitle, inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Equal(5000, model.Title.Length);
        Assert.Equal(longTitle, model.Title);
    }

    [Fact]
    public void Invoke_WithVeryLongButtonText_HandlesCorrectly()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };
        var longButtonText = new string('B', 5000);

        // Act
        var result = component.Invoke("dialogId", "Title", inputs, longButtonText);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Equal(5000, model.ButtonText.Length);
        Assert.Equal(longButtonText, model.ButtonText);
    }

    #endregion

    #region Edge Case Tests - Input Collections

    [Fact]
    public void Invoke_WithSingleInput_ReturnsSingleInputCollection()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "onlyInput", Type = "text" } };

        // Act
        var result = component.Invoke("dialogId", "Title", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Single(model.Inputs);
        Assert.Equal("onlyInput", model.Inputs.First().Id);
    }

    [Fact]
    public void Invoke_WithManyInputs_HandlesLargeCollections()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = Enumerable.Range(1, 100)
            .Select(i => new DialogInputModel { Id = $"input{i}", Type = "text" })
            .ToArray();

        // Act
        var result = component.Invoke("dialogId", "Title", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Equal(100, model.Inputs.Count());
        Assert.Equal("input1", model.Inputs.First().Id);
        Assert.Equal("input100", model.Inputs.Last().Id);
    }

    [Fact]
    public void Invoke_WithInputsContainingNullProperties_PreservesNullValues()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[]
        {
            new DialogInputModel { Id = null, Type = null },
            new DialogInputModel { Id = "valid", Type = "text" },
            new DialogInputModel { Id = null, Type = "email" }
        };

        // Act
        var result = component.Invoke("dialogId", "Title", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        var inputList = model.Inputs.ToList();
        Assert.Equal(3, inputList.Count);
        Assert.Null(inputList[0].Id);
        Assert.Null(inputList[0].Type);
        Assert.Equal("valid", inputList[1].Id);
        Assert.Equal("text", inputList[1].Type);
    }

    [Fact]
    public void Invoke_WithInputsContainingEmptyStrings_PreservesEmptyStrings()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[]
        {
            new DialogInputModel { Id = "", Type = "" },
            new DialogInputModel { Id = "valid", Type = "text" }
        };

        // Act
        var result = component.Invoke("dialogId", "Title", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        var inputList = model.Inputs.ToList();
        Assert.Equal(2, inputList.Count);
        Assert.Equal("", inputList[0].Id);
        Assert.Equal("", inputList[0].Type);
    }

    [Fact]
    public void Invoke_WithDuplicateInputIds_PreservesDuplicates()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[]
        {
            new DialogInputModel { Id = "input1", Type = "text" },
            new DialogInputModel { Id = "input2", Type = "email" },
            new DialogInputModel { Id = "input1", Type = "password" }
        };

        // Act
        var result = component.Invoke("dialogId", "Title", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        var inputList = model.Inputs.ToList();
        Assert.Equal(3, inputList.Count);
        Assert.Equal("input1", inputList[0].Id);
        Assert.Equal("input1", inputList[2].Id);
    }

    #endregion

    #region Model Structure Tests

    [Fact]
    public void Invoke_ReturnsCorrectModelType()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };

        // Act
        var result = component.Invoke("dialogId", "Title", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
    }

    [Fact]
    public void Invoke_ModelIsComponentItself()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };

        // Act
        var result = component.Invoke("dialogId", "Title", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);
        Assert.Same(component, model);
    }

    [Fact]
    public void Invoke_ModelContainsExpectedData()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[]
        {
            new DialogInputModel { Id = "username", Type = "text" },
            new DialogInputModel { Id = "password", Type = "password" }
        };

        // Act
        var result = component.Invoke("loginDialog", "Login Form", inputs, "Login");

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);

        Assert.Equal("loginDialog", model.DialogId);
        Assert.Equal("Login Form", model.Title);
        Assert.Equal(2, model.Inputs.Count());
        Assert.Equal("username", model.Inputs.First().Id);
        Assert.Equal("password", model.Inputs.Last().Id);
        Assert.Equal("Login", model.ButtonText);
    }

    [Fact]
    public void Invoke_InputsAreNotCloned()
    {
        // Arrange
        var component = CreateComponent();
        var originalInputs = new[]
        {
            new DialogInputModel { Id = "input1", Type = "text" },
            new DialogInputModel { Id = "input2", Type = "email" }
        };

        // Act
        var result = component.Invoke("dialogId", "Title", originalInputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);

        // Verify it's the same reference (not cloned)
        Assert.Same(originalInputs, model.Inputs);
    }

    #endregion

    #region Return Type Tests

    [Fact]
    public void Invoke_ReturnsViewViewComponentResult()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };

        // Act
        var result = component.Invoke("dialogId", "Title", inputs);

        // Assert
        Assert.IsType<ViewViewComponentResult>(result);
    }

    [Fact]
    public void Invoke_ReturnsDefaultView()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };

        // Act
        var result = component.Invoke("dialogId", "Title", inputs);

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
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };

        // Act
        var result = component.Invoke("dialogId", "Title", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        Assert.NotNull(viewResult.ViewData);
        Assert.NotNull(viewResult.ViewData.Model);
    }

    #endregion

    #region DialogInputModel Tests

    [Fact]
    public void DialogInputModel_CanBeInstantiatedWithNullValues()
    {
        // Act
        var model = new DialogInputModel();

        // Assert
        Assert.Null(model.Id);
        Assert.Null(model.Type);
    }

    [Fact]
    public void DialogInputModel_CanSetIdProperty()
    {
        // Act
        var model = new DialogInputModel { Id = "testId" };

        // Assert
        Assert.Equal("testId", model.Id);
    }

    [Fact]
    public void DialogInputModel_CanSetTypeProperty()
    {
        // Act
        var model = new DialogInputModel { Type = "email" };

        // Assert
        Assert.Equal("email", model.Type);
    }

    [Fact]
    public void DialogInputModel_SupportsVariousInputTypes()
    {
        // Arrange
        var supportedTypes = new[] { "text", "email", "password", "number", "tel", "url", "date", "time", "checkbox", "radio", "hidden" };

        // Act & Assert
        foreach (var type in supportedTypes)
        {
            var model = new DialogInputModel { Type = type };
            Assert.Equal(type, model.Type);
        }
    }

    #endregion

    #region Integration Tests - Complex Scenarios

    [Fact]
    public void Invoke_WithComplexLoginDialog_HandlesCorrectly()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[]
        {
            new DialogInputModel { Id = "username", Type = "text" },
            new DialogInputModel { Id = "password", Type = "password" },
            new DialogInputModel { Id = "remember", Type = "checkbox" }
        };

        // Act
        var result = component.Invoke(
            dialogId: "login-dialog",
            title: "Please Login",
            inputs: inputs,
            buttonText: "Sign In");

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);

        Assert.Equal("login-dialog", model.DialogId);
        Assert.Equal("Please Login", model.Title);
        Assert.Equal(3, model.Inputs.Count());
        Assert.Equal("Sign In", model.ButtonText);
    }

    [Fact]
    public void Invoke_WithContactFormDialog_HandlesCorrectly()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[]
        {
            new DialogInputModel { Id = "name", Type = "text" },
            new DialogInputModel { Id = "email", Type = "email" },
            new DialogInputModel { Id = "phone", Type = "tel" },
            new DialogInputModel { Id = "message", Type = "text" }
        };

        // Act
        var result = component.Invoke(
            dialogId: "contact-form",
            title: "Contact Us",
            inputs: inputs,
            buttonText: "Send Message");

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);

        Assert.Equal("contact-form", model.DialogId);
        Assert.Equal("Contact Us", model.Title);
        Assert.Equal(4, model.Inputs.Count());
        var inputList = model.Inputs.ToList();
        Assert.Equal("name", inputList[0].Id);
        Assert.Equal("text", inputList[0].Type);
        Assert.Equal("email", inputList[1].Id);
        Assert.Equal("email", inputList[1].Type);
        Assert.Equal("Send Message", model.ButtonText);
    }

    [Fact]
    public void Invoke_WithSimpleConfirmationDialog_HandlesCorrectly()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = Array.Empty<DialogInputModel>();

        // Act
        var result = component.Invoke(
            dialogId: "confirm-delete",
            title: "Are you sure?",
            inputs: inputs,
            buttonText: "Confirm");

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);

        Assert.Equal("confirm-delete", model.DialogId);
        Assert.Equal("Are you sure?", model.Title);
        Assert.Empty(model.Inputs);
        Assert.Equal("Confirm", model.ButtonText);
    }

    [Fact]
    public void Invoke_WithMultiStepFormInputs_PreservesComplexStructure()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[]
        {
            new DialogInputModel { Id = "step1_name", Type = "text" },
            new DialogInputModel { Id = "step1_email", Type = "email" },
            new DialogInputModel { Id = "step2_address", Type = "text" },
            new DialogInputModel { Id = "step2_city", Type = "text" },
            new DialogInputModel { Id = "step2_zip", Type = "text" },
            new DialogInputModel { Id = "step3_payment", Type = "text" },
            new DialogInputModel { Id = "step3_cvv", Type = "password" }
        };

        // Act
        var result = component.Invoke("checkout-wizard", "Checkout", inputs, "Complete Order");

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var model = Assert.IsType<DialogViewComponent>(viewResult.ViewData!.Model);

        Assert.Equal(7, model.Inputs.Count());
        var inputList = model.Inputs.ToList();
        Assert.Equal("step1_name", inputList[0].Id);
        Assert.Equal("step3_cvv", inputList[6].Id);
    }

    #endregion

    #region Architecture Concern Tests

    [Fact]
    public void DialogViewComponent_HasPublicProperties()
    {
        // Act
        var component = CreateComponent();

        // Assert
        Assert.NotNull(component.GetType().GetProperty("DialogId"));
        Assert.NotNull(component.GetType().GetProperty("Title"));
        Assert.NotNull(component.GetType().GetProperty("Inputs"));
        Assert.NotNull(component.GetType().GetProperty("ButtonText"));
    }

    [Fact]
    public void DialogViewComponent_PropertiesArePubliclyWritable()
    {
        // Arrange
        var component = CreateComponent();

        // Act - Direct property assignment (as done in Invoke method)
        component.DialogId = "test";
        component.Title = "test";
        component.Inputs = Array.Empty<DialogInputModel>();
        component.ButtonText = "test";

        // Assert
        Assert.Equal("test", component.DialogId);
        Assert.Equal("test", component.Title);
        Assert.Empty(component.Inputs);
        Assert.Equal("test", component.ButtonText);
    }

    [Fact]
    public void Invoke_PassesComponentItselfAsModel()
    {
        // Arrange
        var component = CreateComponent();
        var inputs = new[] { new DialogInputModel { Id = "input1", Type = "text" } };

        // Act
        var result = component.Invoke("dialogId", "Title", inputs);

        // Assert
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        // Verify the model is the component instance itself
        Assert.Same(component, viewResult.ViewData!.Model);
    }

    #endregion
}
