using System.ComponentModel.DataAnnotations;

namespace TitanTechnologyView.Models
{
    public class EmployeeFormDto
    {
        public int EmployeeId { get; set; }
        [Required]
        public string EmployeeType { get; set; } = string.Empty;
        [Required]
        public string CompanyCode { get; set; } = string.Empty;
        public int? VendorId { get; set; }
        public string? VendorName { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
        public string? AltName { get; set; }
        public int? Age { get; set; }
        public string? SkillSet { get; set; }
        public decimal? Experience { get; set; }
        public string? TimingAvailability { get; set; }
        public string? ContactNumber1 { get; set; }
        public string? ContactNumber2 { get; set; }
        public string? Remarks { get; set; }
        public string? ReferredBy { get; set; }
        public string? CreatedBy { get; set; }
        public IFormFile? NdaFile { get; set; }
        public IFormFile? AadharFile1 { get; set; }
        public string? PanNumber { get; set; }
        public IFormFile? PanFile1 { get; set; }
        public string? AccountNumber1 { get; set; }
        public string? IfscCode1 { get; set; }
        public string? AccountName1 { get; set; }
        public IFormFile? ChequeFile1 { get; set; }
        public IFormFile? AadharFile2 { get; set; }
        public string? PanNumber2 { get; set; }
        public IFormFile? PanFile2 { get; set; }
        public string? AccountNumber2 { get; set; }
        public string? IfscCode2 { get; set; }
        public string? AccountName2 { get; set; }
        public IFormFile? ChequeFile2 { get; set; }

        public string? NdaUpload { get; set; }
        public string? AadharUpload { get; set; }
        public string? PanUpload { get; set; }
        public string? Cheque1Upload { get; set; }
        public string? Aadhar2Upload { get; set; }
        public string? PanUpload2 { get; set; }
        public string? Cheque2Upload { get; set; }
    }
}
