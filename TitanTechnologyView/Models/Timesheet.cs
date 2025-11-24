using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TitanTechnologyView.Models
{
    public class Timesheet
    {
        [Key]
        public int TimesheetId { get; set; }

        [ForeignKey("ProjectMaster")]
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }

        [ForeignKey("EmployeeMaster")]
        public int EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeEmail { get; set; }
        public DateOnly? WorkMonth { get; set; }

        public string? Name { get; set; }

        public string TimesheetType { get; set; }
        public int? rate { get; set; }
        public string unit { get; set; }
        public int? monthlyworkunit { get; set; }
        public int? holiday { get; set; }
        public int? leave { get; set; }
        public int? extradays { get; set; }
        public int? actualworkdayshours { get; set; }
        public string? uploadtimesheet { get; set; }
        public string? status { get; set; }
        public int? approvedworkingunit { get; set; }
        public double? netrate { get; set; }
        public int? tdsapplicable { get; set; }
        public double? netpayble { get; set; }
        public double? netpaybleaftertds { get; set; }
        public string? salarypaid { get; set; }
        public DateOnly? salarydate { get; set; }
        public double? tdavalue { get; set; }
        public string? tdspaid { get; set; }
        public DateOnly? tdsdate { get; set; }

        public ProjectMaster? ProjectMaster { get; set; }
        public EmployeeMaster? EmployeeMaster { get; set; }
        public ICollection<TimeSheetEntry>? Entries { get; set; }

    }
}
