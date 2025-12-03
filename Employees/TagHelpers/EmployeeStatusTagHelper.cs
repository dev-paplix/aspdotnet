namespace Employees.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

[HtmlTargetElement("employee-status")]
public class EmployeeStatusTagHelper : TagHelper
{
   public bool IsActive { get; set; }

   public override void Process(TagHelperContext context, TagHelperOutput output)
   {
        output.TagName = "span";    
        if (IsActive)
        {
            output.Attributes.SetAttribute("class", "badge bg-success");
            output.Content.SetContent("Active");
        }
        else
        {
            output.Attributes.SetAttribute("class", "badge bg-secondary");
            output.Content.SetContent("Inactive");
        }
   }
}