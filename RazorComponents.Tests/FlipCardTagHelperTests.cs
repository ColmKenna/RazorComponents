using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using RazorComponents.TagHelpers.FlipCard;
using Xunit;

namespace RazorComponents.Tests;

public class FlipCardTagHelperTests : TagHelperTestBase<FlipCardTagHelper>
{
    #region Helper Methods

    protected override FlipCardTagHelper CreateTagHelper(Action<FlipCardTagHelper>? configure = null)
    {
        var tagHelper = new FlipCardTagHelper();
        configure?.Invoke(tagHelper);
        return tagHelper;
    }

    /// <summary>
    /// Creates a matched TagHelperContext and TagHelperOutput pair with a shared items dictionary.
    /// The getChildContentAsync delegate simulates child TagHelpers (card-front/card-back)
    /// populating the FlipCardContext with the specified content and titles.
    /// </summary>
    private static (TagHelperContext context, TagHelperOutput output) CreateFlipCardContextAndOutput(
        string? frontContent = null,
        string? backContent = null,
        string? frontTitle = null,
        string? backTitle = null)
    {
        var items = new Dictionary<object, object>();

        var context = new TagHelperContext(
            tagName: "flip-card",
            allAttributes: new TagHelperAttributeList(),
            items: items,
            uniqueId: "test");

        var output = new TagHelperOutput(
            tagName: "flip-card",
            attributes: new TagHelperAttributeList(),
            getChildContentAsync: (useCached, encoder) =>
            {
                // Simulate child TagHelpers populating the FlipCardContext
                if (items.TryGetValue(typeof(FlipCardContext), out var ctx) && ctx is FlipCardContext cardContext)
                {
                    if (frontTitle != null) cardContext.FrontTitle = frontTitle;
                    if (backTitle != null) cardContext.BackTitle = backTitle;
                    if (frontContent != null) cardContext.FrontContent = new HtmlString(frontContent);
                    if (backContent != null) cardContext.BackContent = new HtmlString(backContent);
                }
                return Task.FromResult<TagHelperContent>(new DefaultTagHelperContent());
            })
        {
            TagMode = TagMode.StartTagAndEndTag
        };

        return (context, output);
    }

    #endregion

    #region Property Default Tests

    [Fact]
    public void FlipDirection_DefaultValue_IsHorizontal()
    {
        var tagHelper = new FlipCardTagHelper();
        Assert.Equal(FlipDirection.Horizontal, tagHelper.FlipDirection);
    }

    [Fact]
    public void Size_DefaultValue_IsNull()
    {
        var tagHelper = new FlipCardTagHelper();
        Assert.Null(tagHelper.Size);
    }

    [Fact]
    public void AutoHeight_DefaultValue_IsNull()
    {
        var tagHelper = new FlipCardTagHelper();
        Assert.Null(tagHelper.AutoHeight);
    }

    [Fact]
    public void ButtonText_DefaultValue_IsFlip()
    {
        var tagHelper = new FlipCardTagHelper();
        Assert.Equal("Flip", tagHelper.ButtonText);
    }

    [Fact]
    public void CssClass_DefaultValue_IsNull()
    {
        var tagHelper = new FlipCardTagHelper();
        Assert.Null(tagHelper.CssClass);
    }

    #endregion

    #region FlipCardSize Class Tests

    [Fact]
    public void FlipCardSize_DefaultValues_AreNull()
    {
        var size = new FlipCardSize();
        Assert.Null(size.Width);
        Assert.Null(size.Height);
    }

    [Fact]
    public void FlipCardSize_CanSetWidthAndHeight()
    {
        var size = new FlipCardSize { Width = "300px", Height = "400px" };
        Assert.Equal("300px", size.Width);
        Assert.Equal("400px", size.Height);
    }

    [Fact]
    public void FlipCardSize_CanSetWidthOnly()
    {
        var size = new FlipCardSize { Width = "300px" };
        Assert.Equal("300px", size.Width);
        Assert.Null(size.Height);
    }

    [Fact]
    public void FlipCardSize_CanSetHeightOnly()
    {
        var size = new FlipCardSize { Height = "400px" };
        Assert.Null(size.Width);
        Assert.Equal("400px", size.Height);
    }

    #endregion

    #region Happy Path Tests

    [Fact]
    public async Task ProcessAsync_WithDefaults_SetsOutputTagNameToDiv()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("div", output.TagName);
    }

    [Fact]
    public async Task ProcessAsync_WithDefaults_SetsDefaultContainerClass()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("card-container flip-card", output.Attributes["class"].Value);
    }

    [Fact]
    public async Task ProcessAsync_WithDefaults_GeneratesCardStructure()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var (context, output) = CreateFlipCardContextAndOutput(
            frontContent: "<p>Front</p>",
            backContent: "<p>Back</p>");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("card-front", content);
        Assert.Contains("card-back", content);
        Assert.Contains("card-front-header", content);
        Assert.Contains("card-back-header", content);
        Assert.Contains("card-front-content", content);
        Assert.Contains("card-back-content", content);
    }

    [Fact]
    public async Task ProcessAsync_WithAllProperties_GeneratesCompleteOutput()
    {
        // Arrange
        var tagHelper = CreateTagHelper(th =>
        {
            th.FlipDirection = FlipDirection.Vertical;
            th.Size = new FlipCardSize { Width = "400px", Height = "300px" };
            th.AutoHeight = false;
            th.ButtonText = "Turn Over";
            th.CssClass = "custom-card";
        });
        var (context, output) = CreateFlipCardContextAndOutput(
            frontContent: "<p>Front content</p>",
            backContent: "<p>Back content</p>",
            frontTitle: "Question",
            backTitle: "Answer");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        var style = output.Attributes["style"]?.Value?.ToString();
        Assert.Equal("card-container flip-card custom-card", output.Attributes["class"].Value);
        Assert.Contains("flip-vertical", content);
        Assert.Contains("--flip-card-width: 400px", style!);
        Assert.Contains("--flip-card-height: 300px", style!);
        Assert.Contains("Question", content);
        Assert.Contains("Answer", content);
        Assert.Contains("Turn Over", content);
        Assert.Contains("<p>Front content</p>", content);
        Assert.Contains("<p>Back content</p>", content);
        Assert.DoesNotContain("auto-height", content);
        Assert.DoesNotContain("style=", content); // no inline style on inner card div
    }

    #endregion

    #region FlipDirection Tests

    [Fact]
    public async Task ProcessAsync_WithHorizontalDirection_AddsFlipHorizontalClass()
    {
        // Arrange
        var tagHelper = CreateTagHelper(th => th.FlipDirection = FlipDirection.Horizontal);
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("flip-horizontal", content);
        Assert.DoesNotContain("flip-vertical", content);
    }

    [Fact]
    public async Task ProcessAsync_WithVerticalDirection_AddsFlipVerticalClass()
    {
        // Arrange
        var tagHelper = CreateTagHelper(th => th.FlipDirection = FlipDirection.Vertical);
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("flip-vertical", content);
        Assert.DoesNotContain("flip-horizontal", content);
    }

    #endregion

    #region Size - Width CSS Validation Tests

    [Theory]
    [InlineData("300px")]
    [InlineData("20rem")]
    [InlineData("1.5em")]
    [InlineData("100%")]
    [InlineData("0px")]
    [InlineData("10.25rem")]
    [InlineData("100vh")]
    [InlineData("50vw")]
    [InlineData("10ch")]
    [InlineData("5vmin")]
    [InlineData("5vmax")]
    [InlineData("-10px")]
    [InlineData("-1.5em")]
    [InlineData("300PX")]
    [InlineData("20REM")]
    public async Task ProcessAsync_WithValidSizeWidth_AppliesWidthStyle(string width)
    {
        // Arrange
        var tagHelper = CreateTagHelper(th => th.Size = new FlipCardSize { Width = width });
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert — CSS custom property set on outer container
        var style = output.Attributes["style"]?.Value?.ToString();
        Assert.NotNull(style);
        Assert.Contains($"--flip-card-width: {width}", style);
    }

    [Theory]
    [InlineData("auto")]
    [InlineData("300")]
    [InlineData("px")]
    [InlineData("300 px")]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("300px; background: red")]
    public async Task ProcessAsync_WithInvalidSizeWidth_ThrowsInvalidOperationException(string width)
    {
        // Arrange
        var tagHelper = CreateTagHelper(th => th.Size = new FlipCardSize { Width = width });
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => tagHelper.ProcessAsync(context, output));
        Assert.Contains("width", ex.Message);
    }

    #endregion

    #region Size - Height CSS Validation Tests

    [Theory]
    [InlineData("300px")]
    [InlineData("20rem")]
    [InlineData("1.5em")]
    [InlineData("100%")]
    [InlineData("100vh")]
    [InlineData("-5px")]
    [InlineData("300PX")]
    public async Task ProcessAsync_WithValidSizeHeight_AppliesHeightStyle(string height)
    {
        // Arrange
        var tagHelper = CreateTagHelper(th => th.Size = new FlipCardSize { Height = height });
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert — CSS custom property set on outer container
        var style = output.Attributes["style"]?.Value?.ToString();
        Assert.NotNull(style);
        Assert.Contains($"--flip-card-height: {height}", style);
    }

    [Theory]
    [InlineData("auto")]
    [InlineData("300")]
    [InlineData("px")]
    [InlineData("300 px")]
    [InlineData("<script>alert(1)</script>")]
    public async Task ProcessAsync_WithInvalidSizeHeight_ThrowsInvalidOperationException(string height)
    {
        // Arrange
        var tagHelper = CreateTagHelper(th => th.Size = new FlipCardSize { Height = height });
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => tagHelper.ProcessAsync(context, output));
        Assert.Contains("height", ex.Message);
    }

    #endregion

    #region Size - Combined Width/Height Tests

    [Fact]
    public async Task ProcessAsync_WithBothValidSizeWidthAndHeight_AddsBothStyles()
    {
        // Arrange
        var tagHelper = CreateTagHelper(th =>
            th.Size = new FlipCardSize { Width = "400px", Height = "300px" });
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert — both CSS custom properties on outer container
        var style = output.Attributes["style"]?.Value?.ToString();
        Assert.NotNull(style);
        Assert.Contains("--flip-card-width: 400px", style);
        Assert.Contains("--flip-card-height: 300px", style);
    }

    [Fact]
    public async Task ProcessAsync_WithNoSize_HasNoInlineStyle()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert — no style attribute on container or inner content
        Assert.Null(output.Attributes["style"]);
        var content = GetOutputContent(output);
        Assert.DoesNotContain("style=", content);
    }

    [Fact]
    public async Task ProcessAsync_WithSizeWidthOnly_HasNoHeightStyle()
    {
        // Arrange
        var tagHelper = CreateTagHelper(th =>
            th.Size = new FlipCardSize { Width = "300px" });
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var style = output.Attributes["style"]?.Value?.ToString();
        Assert.NotNull(style);
        Assert.Contains("--flip-card-width: 300px", style);
        Assert.DoesNotContain("--flip-card-height", style);
    }

    [Fact]
    public async Task ProcessAsync_WithSizeHeightOnly_HasNoWidthStyle()
    {
        // Arrange
        var tagHelper = CreateTagHelper(th =>
            th.Size = new FlipCardSize { Height = "300px" });
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var style = output.Attributes["style"]?.Value?.ToString();
        Assert.NotNull(style);
        Assert.Contains("--flip-card-height: 300px", style);
        Assert.DoesNotContain("--flip-card-width", style);
    }

    [Fact]
    public async Task ProcessAsync_WithSizeHavingNullWidthAndHeight_HasNoInlineStyle()
    {
        // Arrange — Size object exists but both properties are null
        var tagHelper = CreateTagHelper(th =>
            th.Size = new FlipCardSize());
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Null(output.Attributes["style"]);
    }

    [Fact]
    public async Task ProcessAsync_WithSizeHavingEmptyWidth_HasNoWidthStyle()
    {
        // Arrange
        var tagHelper = CreateTagHelper(th =>
            th.Size = new FlipCardSize { Width = "" });
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Null(output.Attributes["style"]);
    }

    [Fact]
    public async Task ProcessAsync_WithValidSizeWidthAndInvalidHeight_ThrowsForHeight()
    {
        // Arrange — Width is valid but Height is not; exception thrown for Height
        var tagHelper = CreateTagHelper(th =>
            th.Size = new FlipCardSize { Width = "200px", Height = "invalid" });
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => tagHelper.ProcessAsync(context, output));
        Assert.Contains("height", ex.Message);
    }

    #endregion

    #region AutoHeight Tests

    [Fact]
    public async Task ProcessAsync_WithNoSizeAndNoAutoHeight_EnablesAutoHeight()
    {
        // Arrange — Size defaults to null, AutoHeight defaults to null
        // useAutoHeight = null ?? IsNullOrEmpty(null) = true → auto-fit
        var tagHelper = CreateTagHelper();
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("auto-height", content);
    }

    [Fact]
    public async Task ProcessAsync_WithSizeHeightAndNoAutoHeight_DisablesAutoHeight()
    {
        // Arrange — when Size.Height is set, auto-height should be disabled by default
        var tagHelper = CreateTagHelper(th =>
            th.Size = new FlipCardSize { Height = "300px" });
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.DoesNotContain("auto-height", content);
    }

    [Fact]
    public async Task ProcessAsync_WithExplicitAutoHeightTrue_EnablesAutoHeight()
    {
        // Arrange — explicit AutoHeight=true overrides Size.Height-based default
        var tagHelper = CreateTagHelper(th =>
        {
            th.AutoHeight = true;
            th.Size = new FlipCardSize { Height = "300px" };
        });
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("auto-height", content);
    }

    [Fact]
    public async Task ProcessAsync_WithExplicitAutoHeightFalse_DisablesAutoHeight()
    {
        // Arrange — explicit AutoHeight=false even without Size
        var tagHelper = CreateTagHelper(th => th.AutoHeight = false);
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.DoesNotContain("auto-height", content);
    }

    [Fact]
    public async Task ProcessAsync_WithExplicitAutoHeightFalseAndNoSize_DisablesAutoHeight()
    {
        // Arrange — AutoHeight=false overrides the default (which would enable auto-height when no Size)
        var tagHelper = CreateTagHelper(th =>
        {
            th.AutoHeight = false;
        });
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.DoesNotContain("auto-height", content);
    }

    [Fact]
    public async Task ProcessAsync_WithSizeWidthOnlyAndNoAutoHeight_EnablesAutoHeight()
    {
        // Arrange — Size.Width is set but Size.Height is null → auto-height
        var tagHelper = CreateTagHelper(th =>
            th.Size = new FlipCardSize { Width = "400px" });
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("auto-height", content);
    }

    [Fact]
    public async Task ProcessAsync_WithEmptySizeAndNoAutoHeight_EnablesAutoHeight()
    {
        // Arrange — Size object exists but Width/Height are both null → auto-fit
        var tagHelper = CreateTagHelper(th =>
            th.Size = new FlipCardSize());
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("auto-height", content);
    }

    #endregion

    #region CssClass Tests

    [Fact]
    public async Task ProcessAsync_WithNoCssClass_UsesDefaultContainerClasses()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("card-container flip-card", output.Attributes["class"].Value);
    }

    [Fact]
    public async Task ProcessAsync_WithCssClass_AppendsToContainerClasses()
    {
        // Arrange
        var tagHelper = CreateTagHelper(th => th.CssClass = "custom-class extra");
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("card-container flip-card custom-class extra", output.Attributes["class"].Value);
    }

    [Fact]
    public async Task ProcessAsync_WithEmptyCssClass_UsesDefaultContainerClasses()
    {
        // Arrange
        var tagHelper = CreateTagHelper(th => th.CssClass = "");
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("card-container flip-card", output.Attributes["class"].Value);
    }

    #endregion

    #region ButtonText Tests

    [Fact]
    public async Task ProcessAsync_WithDefaultButtonText_RendersFlip()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains(">Flip</button>", content);
    }

    [Fact]
    public async Task ProcessAsync_WithCustomButtonText_RendersCustomText()
    {
        // Arrange
        var tagHelper = CreateTagHelper(th => th.ButtonText = "Show Answer");
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains(">Show Answer</button>", content);
    }

    #endregion

    #region Content Rendering Tests

    [Fact]
    public async Task ProcessAsync_WithFrontAndBackContent_RendersBothFaces()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var (context, output) = CreateFlipCardContextAndOutput(
            frontContent: "<p>Front side</p>",
            backContent: "<p>Back side</p>");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("<p>Front side</p>", content);
        Assert.Contains("<p>Back side</p>", content);
    }

    [Fact]
    public async Task ProcessAsync_WithNullFrontContent_RendersFrontContentAsEmpty()
    {
        // Arrange — no front content simulated
        var tagHelper = CreateTagHelper();
        var (context, output) = CreateFlipCardContextAndOutput(backContent: "<p>Back</p>");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert — front content area exists but is empty
        var content = GetOutputContent(output);
        Assert.Contains("card-front-content", content);
        Assert.Contains("<p>Back</p>", content);
    }

    [Fact]
    public async Task ProcessAsync_WithFrontAndBackTitles_RendersTitlesInHeaders()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var (context, output) = CreateFlipCardContextAndOutput(
            frontTitle: "Question",
            backTitle: "Answer");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("<h2>Question</h2>", content);
        Assert.Contains("<h2>Answer</h2>", content);
    }

    [Fact]
    public async Task ProcessAsync_WithDefaultTitles_RendersFrontAndBack()
    {
        // Arrange — no titles provided, FlipCardContext defaults apply
        var tagHelper = CreateTagHelper();
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("<h2>Front</h2>", content);
        Assert.Contains("<h2>Back</h2>", content);
    }

    [Fact]
    public async Task ProcessAsync_WithNullFrontTitle_DefaultsToFront()
    {
        // Arrange — explicitly set FrontTitle to null to test the ?? "Front" fallback
        var tagHelper = CreateTagHelper();
        var items = new Dictionary<object, object>();
        var context = new TagHelperContext(
            tagName: "flip-card",
            allAttributes: new TagHelperAttributeList(),
            items: items,
            uniqueId: "test");
        var output = new TagHelperOutput(
            tagName: "flip-card",
            attributes: new TagHelperAttributeList(),
            getChildContentAsync: (useCached, encoder) =>
            {
                if (items.TryGetValue(typeof(FlipCardContext), out var ctx) && ctx is FlipCardContext cardContext)
                {
                    cardContext.FrontTitle = null!;
                }
                return Task.FromResult<TagHelperContent>(new DefaultTagHelperContent());
            })
        {
            TagMode = TagMode.StartTagAndEndTag
        };

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert — should use "Front" fallback from the ?? operator
        var content = GetOutputContent(output);
        Assert.Contains("<h2>Front</h2>", content);
    }

    #endregion

    #region Context/Items State Tests

    [Fact]
    public async Task ProcessAsync_CreatesFlipCardContextInItems()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var items = new Dictionary<object, object>();
        var context = new TagHelperContext(
            tagName: "flip-card",
            allAttributes: new TagHelperAttributeList(),
            items: items,
            uniqueId: "test");
        var output = new TagHelperOutput(
            tagName: "flip-card",
            attributes: new TagHelperAttributeList(),
            getChildContentAsync: (useCached, encoder) =>
                Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()))
        {
            TagMode = TagMode.StartTagAndEndTag
        };

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.True(items.ContainsKey(typeof(FlipCardContext)));
        Assert.IsType<FlipCardContext>(items[typeof(FlipCardContext)]);
    }

    [Fact]
    public async Task ProcessAsync_FlipCardContextIsAvailableToChildTagHelpers()
    {
        // Arrange
        FlipCardContext? capturedContext = null;
        var items = new Dictionary<object, object>();
        var tagHelper = CreateTagHelper();
        var context = new TagHelperContext(
            tagName: "flip-card",
            allAttributes: new TagHelperAttributeList(),
            items: items,
            uniqueId: "test");
        var output = new TagHelperOutput(
            tagName: "flip-card",
            attributes: new TagHelperAttributeList(),
            getChildContentAsync: (useCached, encoder) =>
            {
                // Capture the context to verify it was set before child processing
                if (items.TryGetValue(typeof(FlipCardContext), out var ctx))
                {
                    capturedContext = ctx as FlipCardContext;
                }
                return Task.FromResult<TagHelperContent>(new DefaultTagHelperContent());
            })
        {
            TagMode = TagMode.StartTagAndEndTag
        };

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert — context should have been available during child processing
        Assert.NotNull(capturedContext);
        Assert.Equal("Front", capturedContext!.FrontTitle);
        Assert.Equal("Back", capturedContext.BackTitle);
    }

    #endregion

    #region XSS/Encoding Tests

    [Fact]
    public async Task ProcessAsync_WithHtmlInFrontTitle_EncodesTitle()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var (context, output) = CreateFlipCardContextAndOutput(
            frontTitle: "<script>alert('xss')</script>");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.DoesNotContain("<script>", content);
        Assert.Contains("&lt;script&gt;", content);
    }

    [Fact]
    public async Task ProcessAsync_WithHtmlInBackTitle_EncodesTitle()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var (context, output) = CreateFlipCardContextAndOutput(
            backTitle: "<img src=x onerror=alert(1)>");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.DoesNotContain("<img", content);
        Assert.Contains("&lt;img", content);
    }

    [Fact]
    public async Task ProcessAsync_WithHtmlInButtonText_EncodesButtonText()
    {
        // Arrange
        var tagHelper = CreateTagHelper(th => th.ButtonText = "<b>Click</b>");
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.DoesNotContain("<b>Click</b>", content);
        Assert.Contains("&lt;b&gt;Click&lt;/b&gt;", content);
    }

    [Theory]
    [InlineData("Test & More", "&amp;")]
    [InlineData("Test \"quoted\"", "&quot;")]
    [InlineData("Test 'single'", "&#x27;")]
    [InlineData("Test < Less", "&lt;")]
    [InlineData("Test > Greater", "&gt;")]
    public async Task ProcessAsync_WithSpecialCharactersInTitle_EncodesCorrectly(
        string title, string expectedEncoded)
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var (context, output) = CreateFlipCardContextAndOutput(frontTitle: title);

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains(expectedEncoded, content);
    }

    #endregion

    #region HTML Structure Tests

    [Fact]
    public async Task ProcessAsync_FrontFace_HasAriaHiddenFalse()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("class=\"card-front\" aria-hidden=\"false\"", content);
    }

    [Fact]
    public async Task ProcessAsync_BackFace_HasAriaHiddenTrue()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("class=\"card-back\" aria-hidden=\"true\"", content);
    }

    [Fact]
    public async Task ProcessAsync_FlipButtons_HaveCorrectAttributes()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        Assert.Contains("type=\"button\"", content);
        Assert.Contains("class=\"rotate-button\"", content);
        Assert.Contains("data-flip-card-button", content);
        Assert.Contains("aria-pressed=\"false\"", content);
    }

    [Fact]
    public async Task ProcessAsync_RendersTwoFlipButtons()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert — one button in front header, one in back header
        var content = GetOutputContent(output);
        var buttonCount = System.Text.RegularExpressions.Regex.Matches(content, "data-flip-card-button").Count;
        Assert.Equal(2, buttonCount);
    }

    [Fact]
    public async Task ProcessAsync_FrontContentAppearsBeforeBackContent()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var (context, output) = CreateFlipCardContextAndOutput(
            frontContent: "FRONT_MARKER",
            backContent: "BACK_MARKER");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = GetOutputContent(output);
        var frontIndex = content.IndexOf("FRONT_MARKER", StringComparison.Ordinal);
        var backIndex = content.IndexOf("BACK_MARKER", StringComparison.Ordinal);
        Assert.True(frontIndex < backIndex, "Front content should appear before back content");
    }

    [Fact]
    public async Task ProcessAsync_CardClassesIncludeFlipDirectionAndAutoHeight()
    {
        // Arrange
        var tagHelper = CreateTagHelper(th => th.FlipDirection = FlipDirection.Vertical);
        var (context, output) = CreateFlipCardContextAndOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert — inner card div should have combined classes
        var content = GetOutputContent(output);
        Assert.Contains("class=\"card flip-vertical auto-height\"", content);
    }

    #endregion
}
