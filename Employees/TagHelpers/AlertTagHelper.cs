namespace Employees.TagHelpers;

using Microsoft.AspNetCore.Razor.TagHelpers;

[HtmlTargetElement("alert")]
public class AlertTagHelper : TagHelper
{
    public string Type {get; set;} = "info";
    public bool Dismissible {get; set;} = true;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        
        var cssClass = $"alert alert-{Type}";
        if (Dismissible)
        {
            cssClass += "alert-dismissible fade show";
        }

        output.Attributes.SetAttribute("class", cssClass);
        output.Attributes.SetAttribute("role", "alert");

        if (Dismissible)
        {
            output.PreContent.SetHtmlContent(
                @"<button type=""button"" class=""btn-close"" data-bs-dismiss=""alert"" aria-label=""Close""></button>");
        }
    }
}