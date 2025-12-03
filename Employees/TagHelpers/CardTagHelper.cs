namespace Employees.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

[HtmlTargetElement("card")]
public class CardTagHelper : TagHelper
{
    public string? Title { get; set; }
    public string? HeaderClass { get; set; } = "bg-primary text-white";
    public string? BodyClass { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("class", "card");

        var content = output.GetChildContentAsync().Result.GetContent();

        var html = "";
        if (!string.IsNullOrEmpty(Title))
        {
            html += $@"<div class=""card-header {HeaderClass}"">
                        <h5 class=""card-title mb-0"">{Title}</h5>
                      </div>";
        }

        html += $@"<div class=""card-body {BodyClass}"">
                    {content}
                   </div>";

        output.Content.SetHtmlContent(html);
    }
}   