using Microsoft.AspNetCore.Razor.TagHelpers;

namespace VanDerTil.ContosoUniversity.Web.Features.Shared;

[HtmlTargetElement("validation-summary", TagStructure = TagStructure.NormalOrSelfClosing)]
public class ValidationSummaryTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagMode = TagMode.StartTagAndEndTag;
        output.TagName = "div";
        output.Attributes.SetAttribute("class", "validation-summary");
        output.Attributes.SetAttribute("data-validation-summary", null);
    }
}
