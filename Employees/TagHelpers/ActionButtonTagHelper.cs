namespace Employees.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

[HtmlTargetElement("action-button")]
public class ActionButtonTagHelper : TagHelper
{
    public string Controller {get; set;} = string.Empty;
    public string Action {get; set;} = string.Empty;
    public string? RouteId {get; set; }
    public string ButtonType {get; set;} = "primary";
    public string? Icon {get; set;}
    public string Text {get; set;} = "Button";

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext? ViewContext {get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "a";
        output.Attributes.SetAttribute("class", $"btn btn-{ButtonType}");

        var url = $"/{Controller}/{Action}";
        if (!string.IsNullOrEmpty(RouteId))
        {
            url += $"/{RouteId}";
        }

        output.Attributes.SetAttribute("href", url);

        var content = "";
        if (!string.IsNullOrEmpty(Icon))
        {
            content += $@"<i class=""bi bi-{Icon}""></i> ";
        }
        content += Text;

        output.Content.SetHtmlContent(content);
    }
}