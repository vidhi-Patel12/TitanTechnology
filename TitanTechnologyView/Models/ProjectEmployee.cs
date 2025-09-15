using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TitanTechnologyView.Models
{
    public class ProjectEmployee
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ProjectMaster")]
        public string ProjectCode { get; set; }

        [ForeignKey("EmployeeMaster")]
        public int EmployeeId { get; set; }
        public string? Name { get; set; }
        public string EmployeeType { get; set; }
        public string Technology { get; set; }
        public string AllocationType { get; set; }
        public string CommissionType { get; set; }
        public decimal? ConsultantRate { get; set; }
        public string RateUnit { get; set; }
        public string TimesheetType { get; set; }
        public string SapModule { get; set; }
        public DateTime? EmployeeStartDate { get; set; }
        public DateTime? EmployeeEndDate { get; set; }
        public int? CycleStartDay { get; set; }
        public int? CycleEndDay { get; set; }
        public string PaymentMode { get; set; }
        public bool? TimesheetRequired { get; set; }
        public decimal? CustomerRate { get; set; }
        public bool? Inactive { get; set; }
        public int? SalaryPaymentDays { get; set; }
        public decimal? TdsPercent { get; set; }
        public string AccountPreference { get; set; }
        public string TextFields { get; set; }
        public ProjectMaster? ProjectMaster { get; set; }
        public EmployeeMaster? EmployeeMaster { get; set; }
        public CompanyMaster? companyMaster { get; set; }
    }
}
