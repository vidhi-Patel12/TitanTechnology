using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TitanTechnologyView.Models
{
    public class ProjectMaster
    {
        [Key]
        [MaxLength(20)]
        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? CustomerId { get; set; }
        public decimal? CustomerRate { get; set; }
        public string RateUnit { get; set; }
        public string HsCode { get; set; }
        public string Notes { get; set; }
        public string ApprovalLevel { get; set; }
        public string Status { get; set; }

        [ForeignKey("CustomerId")]
        public CustomerMaster? CustomerMaster { get; set; }

        public ICollection<ProjectEmployee>? ProjectEmployees { get; set; }
        public ICollection<Timesheet>? Timesheets { get; set; }

    }
}
