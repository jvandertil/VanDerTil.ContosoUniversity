using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace VanDerTil.ContosoUniversity.Web.Features.Shared;

/// <summary>
/// A custom tag helper that renders a complete form field with label, input, and validation message.
/// </summary>
/// <remarks>
/// Usage: &lt;form-field for="PropertyName" label="@Localizer["Label"]" required /&gt;
/// 
/// This tag helper combines three built-in ASP.NET Core tag helpers to create a reusable form field component
/// with consistent styling and behavior.
/// </remarks>
[HtmlTargetElement("form-field", TagStructure = TagStructure.NormalOrSelfClosing)]
public class FormFieldTagHelper : TagHelper
{
    private const string LabelCssClass = "form-field__label";
    private const string InputCssClass = "form-field__input";
    private const string ErrorCssClass = "form-field__error";
    private const string RequiredIndicatorHtml = @"<span aria-hidden=""true"" class=""form-field__required"">*</span>";

    private readonly IHtmlGenerator _htmlGenerator;

    /// <summary>
    /// Gets or sets the model property to bind to (corresponds to asp-for).
    /// </summary>
    [HtmlAttributeName("for")]
    public ModelExpression? For { get; set; }

    /// <summary>
    /// Gets or sets the label text content (can be a localized string).
    /// </summary>
    [HtmlAttributeName("label")]
    public IHtmlContent? Label { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to display a required indicator.
    /// </summary>
    [HtmlAttributeName("required")]
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets the current view context.
    /// </summary>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext? ViewContext { get; set; }

    public FormFieldTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (For is null || ViewContext is null)
        {
            return;
        }

        ConfigureContainer(output);

        var content = new HtmlContentBuilder();
        content.AppendHtml(await ProcessLabelAsync());
        content.AppendHtml(await ProcessInputAsync());
        content.AppendHtml(await ProcessValidationAsync());

        output.Content.SetHtmlContent(content);
    }

    /// <summary>
    /// Configures the outer container div with styling.
    /// </summary>
    private static void ConfigureContainer(TagHelperOutput output)
    {
        output.TagMode = TagMode.StartTagAndEndTag;
        output.TagName = "div";
        output.AddClass("form-field", HtmlEncoder.Default);
    }

    /// <summary>
    /// Processes and renders the label element using LabelTagHelper.
    /// </summary>
    private async Task<TagHelperOutput> ProcessLabelAsync()
    {
        var labelTagHelper = new LabelTagHelper(_htmlGenerator)
        {
            For = For,
            ViewContext = ViewContext
        };

        var labelOutput = CreateTagHelperOutput("label");
        labelOutput.AddClass(LabelCssClass, HtmlEncoder.Default);
        labelOutput.Content.SetHtmlContent(BuildLabelContent());

        await labelTagHelper.ProcessAsync(
            CreateTagHelperContext("label"),
            labelOutput
        );

        return labelOutput;
    }

    /// <summary>
    /// Builds the label content including the optional required indicator.
    /// </summary>
    private IHtmlContent BuildLabelContent()
    {
        var content = new HtmlContentBuilder();

        if (Label is not null)
        {
            content.AppendHtml(Label);
        }

        if (Required)
        {
            content.AppendHtml(RequiredIndicatorHtml);
        }

        return content;
    }

    /// <summary>
    /// Processes and renders the input element using InputTagHelper.
    /// </summary>
    private async Task<TagHelperOutput> ProcessInputAsync()
    {
        var inputTagHelper = new InputTagHelper(_htmlGenerator)
        {
            For = For,
            ViewContext = ViewContext
        };

        var inputOutput = CreateTagHelperOutput("input");

        await inputTagHelper.ProcessAsync(
            CreateTagHelperContext("input"),
            inputOutput
        );

        inputOutput.AddClass(InputCssClass, HtmlEncoder.Default);
        return inputOutput;
    }

    /// <summary>
    /// Processes and renders the validation message element using ValidationMessageTagHelper.
    /// </summary>
    private async Task<TagHelperOutput> ProcessValidationAsync()
    {
        var validationTagHelper = new ValidationMessageTagHelper(_htmlGenerator)
        {
            For = For,
            ViewContext = ViewContext
        };

        var validationOutput = CreateTagHelperOutput("span");

        await validationTagHelper.ProcessAsync(
            CreateTagHelperContext("span"),
            validationOutput
        );

        validationOutput.AddClass(ErrorCssClass, HtmlEncoder.Default);
        return validationOutput;
    }

    /// <summary>
    /// Creates a new TagHelperOutput with an empty content provider.
    /// </summary>
    private static TagHelperOutput CreateTagHelperOutput(string tagName)
    {
        return new TagHelperOutput(
            tagName,
            new TagHelperAttributeList(),
            EmptyTagHelperContentProvider
        );
    }

    /// <summary>
    /// Creates a new TagHelperContext for processing nested tag helpers.
    /// </summary>
    private static TagHelperContext CreateTagHelperContext(string tagName)
    {
        return new TagHelperContext(
            tagName,
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString()
        );
    }

    /// <summary>
    /// Provides an empty TagHelperContent for nested tag helper processing.
    /// </summary>
    private static Func<bool, HtmlEncoder, Task<TagHelperContent>> EmptyTagHelperContentProvider =>
        async (_, _) => new DefaultTagHelperContent();
}
