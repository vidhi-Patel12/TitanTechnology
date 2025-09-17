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

        [ForeignKey("EmployeeMaster")]
        public int EmployeeId { get; set; }

        public string? Name { get; set; }

        public string TimesheetType { get; set; }
        public string MonthYear { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
        public string Status { get; set; }

        public ProjectMaster? ProjectMaster { get; set; }
        public EmployeeMaster? EmployeeMaster { get; set; }
        public ICollection<TimeSheetEntry>? Entries { get; set; }

    }
}
