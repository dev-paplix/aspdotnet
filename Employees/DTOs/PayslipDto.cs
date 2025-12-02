namespace Employees.DTOs;

public class PayslipCreateDto
{
    public decimal Salary { get; set; }
    public decimal MiscAllowance { get; set; }
    public decimal Pension { get; set; }
    public decimal HealthInsurance { get; set; }
    public decimal FamilyBenefit { get; set; }
    public decimal Housing { get; set; }
    public decimal ProfessionalAllowance { get; set; }
}

public class PayslipUpdateDto
{
    public decimal? Salary { get; set; }
    public decimal? MiscAllowance { get; set; }
    public decimal? Pension { get; set; }
    public decimal? HealthInsurance { get; set; }
    public decimal? FamilyBenefit { get; set; }
    public decimal? Housing { get; set; }
    public decimal? ProfessionalAllowance { get; set; }
}

public class PayslipResponseDto
{
    public int Id { get; set; }
    public decimal Salary { get; set; }
    public decimal MiscAllowance { get; set; }
    public decimal Pension { get; set; }
    public decimal HealthInsurance { get; set; }
    public decimal FamilyBenefit { get; set; }
    public decimal Housing { get; set; }
    public decimal ProfessionalAllowance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
