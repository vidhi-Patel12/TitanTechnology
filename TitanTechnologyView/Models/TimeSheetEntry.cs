using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TitanTechnologyView.Models
{
    public class TimeSheetEntry
    {
        [Key]
        public int EntryId { get; set; }

        [ForeignKey("Timesheet")]
        public int TimesheetId { get; set; }

        //public string? ProjectCode { get; set; }   // <-- Add this
        //public string? EmployeeName { get; set; }  // <-- Add this
        public string? DisplayText { get; set; }
        public DateTime? EntryDate { get; set; }
        public string DayName { get; set; }
        public decimal? HoursWorked { get; set; }
        public string ShortDescription { get; set; }
        public string Location { get; set; }
        public string Attendance { get; set; }
        public string ExtraDay { get; set; }
        public Timesheet? Timesheet { get; set; }

    }
}
