namespace Employees.ViewModels;
using System.ComponentModel.DataAnnotations;

public class EmployeeViewModel
{
    public int Id {get; set; }

    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, ErrorMessage = "First Name cannot exceed 100 charecters")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, ErrorMessage = "Last Name cannot exceed 100 charecters")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string Email { get; set; }



    [Phone(ErrorMessage = "Invalid Phone Number")]
    [StringLength(25, ErrorMessage = "Phone number cannot exceed 25 characters")]
    public string Phone { get; set; }

    [Required(ErrorMessage = "Position is required")]
    [StringLength(100, ErrorMessage = "Position cannot exceed 100 characters")]
    public string Position { get; set; }

    [Required(ErrorMessage = "Department is required")]
    [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
    public string Department { get; set; }

    [Required(ErrorMessage = "Hire Date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Hire Date")]
    public DateTime HireDate { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive number")]
    [DataType(DataType.Currency)]
    [Display(Name = "Salary")]
    [Required(ErrorMessage = "Salary is required")]
    public decimal Salary { get; set; }

    [Display(Name = "Active")]    
    public bool IsActive { get; set; }

    [Display(Name = "Created At")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Updated At")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
    public DateTime UpdatedAt { get; set; }

    [Display(Name = "Full Name")]
    public string FullName => $"{FirstName} {LastName}";
}