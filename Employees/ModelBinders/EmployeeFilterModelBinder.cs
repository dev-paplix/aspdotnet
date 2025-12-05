namespace Employees.ModelBinders;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Employees.ViewModels;
using System.Data;

public class EmployeeFilterModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        var model = new EmployeeFilterViewModel();

        //Binding SearchTerm
        var searchTermValue = bindingContext.ValueProvider.GetValue("searchTerm");
        if (searchTermValue != ValueProviderResult.None)
        {
            model.SearchTerm = searchTermValue.FirstValue;
        }

        // Binding Department
        var departmentValue = bindingContext.ValueProvider.GetValue("department");
        if (departmentValue != ValueProviderResult.None)
        {
            model.Department = departmentValue.FirstValue;
        }

        // Bind Minsalary with validation
        var minSalaryValue = bindingContext.ValueProvider.GetValue("minSalary");
        if (minSalaryValue != ValueProviderResult.None &&
            decimal.TryParse(minSalaryValue.FirstValue, out var minSalary))
        {
            model.MinSalary = minSalary >=0 ? minSalary : null;
        }

        // Bind Maxsalary with validation
        var maxSalaryValue = bindingContext.ValueProvider.GetValue("maxSalary");
        if (maxSalaryValue != ValueProviderResult.None &&
            decimal.TryParse(maxSalaryValue.FirstValue, out var maxSalary))
        {
            model.MaxSalary = maxSalary >=0 ? maxSalary : null;
        }
        
        // Binding HireDateFrom
        var hireDateFromValue = bindingContext.ValueProvider.GetValue("hireDateFrom");
        if (hireDateFromValue != ValueProviderResult.None &&
            DateTime.TryParse(hireDateFromValue.FirstValue, out var hireDateFrom))
        {
            model.HireDateFrom = hireDateFrom;
        }

        // Binding HireDateTo
        var hireDateToValue = bindingContext.ValueProvider.GetValue("hireDateTo");
        if (hireDateToValue != ValueProviderResult.None &&
            DateTime.TryParse(hireDateToValue.FirstValue, out var hireDateTo))
        {
            model.HireDateTo = hireDateTo;
        }

        // Binding IcludeInactive
        var includInactiveValue = bindingContext.ValueProvider.GetValue("includeInactive");
        if (includInactiveValue != ValueProviderResult.None)
        {
            model.IncludeInactive = includInactiveValue.FirstValue?.ToLower() == "true";
        }

        // Binding PageNumber with default

        var pageNumberValue = bindingContext.ValueProvider.GetValue("pageNumber");
        if (pageNumberValue != ValueProviderResult.None &&
            int.TryParse(pageNumberValue.FirstValue, out var pageNumber))
        {
            model.PageNumber = pageNumber > 0 ? pageNumber : 1;
        }

        // Bind PageSize with Constraints
        var pageSizeValue = bindingContext.ValueProvider.GetValue("pageSize");
        if (pageSizeValue != ValueProviderResult.None &&
            int.TryParse(pageSizeValue.FirstValue, out var pageSize))
        {
            // Limit page size between 5 and 100
            model.PageSize = Math.Max(5, Math.Min(100, pageSize));
        }

        //Bind SortBy with validation

        var sortByValue = bindingContext.ValueProvider.GetValue("sortBy");
        if (sortByValue != ValueProviderResult.None)
        {
            var allowedSortFields = new[] { "FirstName", "LastName", "Email", "Department", "Salary", "HireDate"};
            var sortBy = sortByValue.FirstValue ?? "LastName";
            model.SortBy = allowedSortFields.Contains(sortBy) ? sortBy : "LastName";
        }

        var sortOrderValue = bindingContext.ValueProvider.GetValue("sortOrder");
        if (sortOrderValue != ValueProviderResult.None)
        {
            var sortOrder = sortOrderValue.FirstValue?.ToLower();
            model.SortOrder = sortOrder == "desc" ? "desc" : "asc";
        }

        bindingContext.Result = ModelBindingResult.Success(model);
        return Task.CompletedTask;
    }
}

public class EmployeeFilterModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }
        if (context.Metadata.ModelType == typeof(EmployeeFilterViewModel))
        {
            return new EmployeeFilterModelBinder();
        }

        return null;
    }

}