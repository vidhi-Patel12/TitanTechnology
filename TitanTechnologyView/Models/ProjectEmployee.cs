using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TitanTechnologyView.Models
{
    public class ProjectEmployee
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Project Code is required")]
        [ForeignKey("ProjectMaster")]
        public string ProjectCode { get; set; }

        [ForeignKey("EmployeeMaster")]
        public int EmployeeId { get; set; }
        public string? Name { get; set; }

        [Required(ErrorMessage = "Employee Type is required")]
        public string EmployeeType { get; set; }

        [Required(ErrorMessage = "Technology is required")]        
        public string Technology { get; set; }

        [Required(ErrorMessage = "Allocation Type is required")]
        public string AllocationType { get; set; }

        [Required(ErrorMessage = "Commission Type is required")]
        public string CommissionType { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Consultant Rate must be a valid number")]
        public decimal? ConsultantRate { get; set; }

        [Required(ErrorMessage = "Rate Unit is required")]
        public string RateUnit { get; set; }

        [Required(ErrorMessage = "Timesheet Type is required")]
        public string TimesheetType { get; set; }

        [Required(ErrorMessage = "SAP Module is required")]
        public string SapModule { get; set; }

        [Required(ErrorMessage = "Start Date is required")]
        public DateTime? EmployeeStartDate { get; set; }

        [Required(ErrorMessage = "End Date is required")]
        public DateTime? EmployeeEndDate { get; set; }
        public int? CycleStartDay { get; set; }
        public int? CycleEndDay { get; set; }

        [Required(ErrorMessage = "Payment Mode is required")]
        public string PaymentMode { get; set; }
        public bool? TimesheetRequired { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Customer Rate must be valid")]
        public decimal? CustomerRate { get; set; }
        public bool? Inactive { get; set; }

        [Range(1, 31, ErrorMessage = "Salary Payment Days must be between 1 and 31")]
        public int? SalaryPaymentDays { get; set; }

        [Range(0, 100, ErrorMessage = "TDS Percent must be between 0 and 100")]
        public decimal? TdsPercent { get; set; }

        [Required(ErrorMessage = "Account Preference is required")]
        public string AccountPreference { get; set; }

        [Required(ErrorMessage = "Text Fields is required")]
        public string TextFields { get; set; }
        public ProjectMaster? ProjectMaster { get; set; }
        public EmployeeMaster? EmployeeMaster { get; set; }
        public CompanyMaster? companyMaster { get; set; }
    }
}
