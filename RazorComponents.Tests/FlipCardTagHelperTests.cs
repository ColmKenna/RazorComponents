using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;
using RazorComponents.TagHelpers.FlipCard;
using Xunit;

namespace RazorComponents.Tests;

public class FlipCardTagHelperTests : TagHelperTestBase<FlipCardTagHelper>
{
    protected override FlipCardTagHelper CreateTagHelper(Action<FlipCardTagHelper>? configure = null)
    {
        var tagHelper = new FlipCardTagHelper();
        configure?.Invoke(tagHelper);
        return tagHelper;
    }

    private static TagHelperContext CreateFlipCardContext(TagHelperAttributeList? attributes = null)
    {
        return CreateContext("flip-card", attributes);
    }

    private static TagHelperOutput CreateFlipCardOutput(
        TagHelperContext context,
        Action<FlipCardContext>? configureContext = null)
    {
        return new TagHelperOutput(
            "flip-card",
            new TagHelperAttributeList(),
            (useCached, encoder) =>
            {
                if (context.Items.TryGetValue(typeof(FlipCardContext), out var item) &&
                    item is FlipCardContext cardContext)
                {
                    configureContext?.Invoke(cardContext);
                }

                var content = new DefaultTagHelperContent();
                content.SetHtmlContent(string.Empty);
                return Task.FromResult<TagHelperContent>(content);
            })
        {
            TagMode = TagMode.StartTagAndEndTag
        };
    }

    private static void PopulateDefaultCardContext(FlipCardContext cardContext)
    {
        cardContext.FrontTitle = "Front Title";
        cardContext.BackTitle = "Back Title";
        cardContext.FrontContent = "<p>Front</p>";
        cardContext.BackContent = "<p>Back</p>";
    }

    private static async Task<(TagHelperOutput output, string content, TagHelperContext context)> ProcessAsync(
        FlipCardTagHelper tagHelper,
        Action<FlipCardContext>? configureContext = null)
    {
        var context = CreateFlipCardContext();
        var output = CreateFlipCardOutput(context, configureContext ?? PopulateDefaultCardContext);

        await tagHelper.ProcessAsync(context, output);

        using var writer = new StringWriter();
        output.Content.WriteTo(writer, HtmlEncoder.Default);
        return (output, writer.ToString(), context);
    }

    [Fact]
    public void Constructor_Defaults_SetAsExpected()
    {
        // Arrange & Act
        var tagHelper = new FlipCardTagHelper();

        // Assert
        Assert.Equal(FlipDirection.Horizontal, tagHelper.FlipDirection);
        Assert.Null(tagHelper.Width);
        Assert.Null(tagHelper.Height);
        Assert.Null(tagHelper.AutoHeight);
        Assert.Equal("Flip", tagHelper.ButtonText);
        Assert.Null(tagHelper.CssClass);
    }

    [Fact]
    public async Task ProcessAsync_WithDefaultSettings_RendersContainerAndCardMarkup()
    {
        // Arrange
        var tagHelper = CreateTagHelper();

        // Act
        var (output, content, context) = await ProcessAsync(tagHelper);

        // Assert
        Assert.Equal("div", output.TagName);
        Assert.Equal(TagMode.StartTagAndEndTag, output.TagMode);
        Assert.Equal("card-container", output.Attributes["class"].Value);
        Assert.Contains("card flip-horizontal auto-height", content);
        Assert.Contains("<h2>Front Title</h2>", content);
        Assert.Contains("<h2>Back Title</h2>", content);
        Assert.Contains("<p>Front</p>", content);
        Assert.Contains("<p>Back</p>", content);
        Assert.Contains("class=\"rotate-button\">Flip</button>", content);
        Assert.True(context.Items.ContainsKey(typeof(FlipCardContext)));
    }

    [Fact]
    public async Task ProcessAsync_WithVerticalDirection_UsesVerticalClass()
    {
        // Arrange
        var tagHelper = CreateTagHelper(th => th.FlipDirection = FlipDirection.Vertical);

        // Act
        var (_, content, _) = await ProcessAsync(tagHelper);

        // Assert
        Assert.Contains("card flip-vertical", content);
        Assert.DoesNotContain("flip-horizontal", content);
    }

    [Fact]
    public async Task ProcessAsync_WithWidthAndHeight_SetsInlineStyleWithoutAutoHeight()
    {
        // Arrange
        var tagHelper = CreateTagHelper(th =>
        {
            th.Width = "320px";
            th.Height = "240px";
        });

        // Act
        var (_, content, _) = await ProcessAsync(tagHelper);

        // Assert
        Assert.Contains("width: 320px;", content);
        Assert.Contains("height: 240px;", content);
        Assert.DoesNotContain("auto-height", content);
    }

    [Fact]
    public async Task ProcessAsync_WithAutoHeightTrueAndHeightSet_IncludesAutoHeightClass()
    {
        // Arrange
        var tagHelper = CreateTagHelper(th =>
        {
            th.Height = "240px";
            th.AutoHeight = true;
        });

        // Act
        var (_, content, _) = await ProcessAsync(tagHelper);

        // Assert
        Assert.Contains("height: 240px;", content);
        Assert.Contains("auto-height", content);
    }

    [Fact]
    public async Task ProcessAsync_WithAutoHeightFalseAndHeightNull_ExcludesAutoHeightClass()
    {
        // Arrange
        var tagHelper = CreateTagHelper(th => th.AutoHeight = false);

        // Act
        var (_, content, _) = await ProcessAsync(tagHelper);

        // Assert
        Assert.DoesNotContain("auto-height", content);
    }

    [Fact]
    public async Task ProcessAsync_WithCustomButtonText_UsesCustomText()
    {
        // Arrange
        var tagHelper = CreateTagHelper(th => th.ButtonText = "Rotate");

        // Act
        var (_, content, _) = await ProcessAsync(tagHelper);

        // Assert
        Assert.Contains("class=\"rotate-button\">Rotate</button>", content);
    }

    [Fact]
    public async Task ProcessAsync_WithCustomCssClass_AppendsClassToContainer()
    {
        // Arrange
        var tagHelper = CreateTagHelper(th => th.CssClass = "extra-class");

        // Act
        var (output, _, _) = await ProcessAsync(tagHelper);

        // Assert
        Assert.Equal("card-container extra-class", output.Attributes["class"].Value);
    }

    [Fact]
    public async Task ProcessAsync_WhenCssClassEmptyString_UsesDefaultContainerClass()
    {
        // Arrange
        var tagHelper = CreateTagHelper(th => th.CssClass = string.Empty);

        // Act
        var (output, _, _) = await ProcessAsync(tagHelper);

        // Assert
        Assert.Equal("card-container", output.Attributes["class"].Value);
    }

    [Fact]
    public async Task ProcessAsync_WithEmptyWidth_DoesNotRenderWidthStyle()
    {
        // Arrange
        var tagHelper = CreateTagHelper(th => th.Width = string.Empty);

        // Act
        var (_, content, _) = await ProcessAsync(tagHelper);

        // Assert
        Assert.DoesNotContain("width:", content);
    }

    [Fact]
    public async Task ProcessAsync_SetsFlipCardContextInItems()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var context = CreateFlipCardContext();
        var output = CreateFlipCardOutput(context, PopulateDefaultCardContext);

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.True(context.Items.TryGetValue(typeof(FlipCardContext), out var item));
        Assert.IsType<FlipCardContext>(item);
    }

    [Fact]
    public async Task ProcessAsync_DoesNotWriteToOtherOutputBuffers()
    {
        // Arrange
        var tagHelper = CreateTagHelper();

        // Act
        var (output, _, _) = await ProcessAsync(tagHelper);

        // Assert
        Assert.Empty(GetPreContent(output));
        Assert.Empty(GetPostContent(output));
        Assert.Empty(output.PreElement.GetContent());
        Assert.Empty(output.PostElement.GetContent());
    }
}
