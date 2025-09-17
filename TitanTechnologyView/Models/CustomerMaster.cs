using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TitanTechnologyView.Models
{
    public class CustomerMaster
    {
        [Key]
        public int CustomerId { get; set; }

        [ForeignKey("CompanyMaster")]
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string Gstn { get; set; }
        public string PanNumber { get; set; }
        public string ContactPersonName { get; set; }
        public string ContactPersonNumber { get; set; }
        public string PaymentTerms { get; set; }
        public string? Agreement1 { get; set; }
        public string? Agreement2 { get; set; }
        public string? Agreement3 { get; set; }
        public string? Agreement4 { get; set; }
        public CompanyMaster? CompanyMaster { get; set; }
        public List<ProjectMaster>? Projects { get; set; }
    }
}
    