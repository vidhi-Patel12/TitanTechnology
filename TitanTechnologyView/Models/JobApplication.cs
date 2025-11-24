namespace TitanTechnologyView.Models
{
    public class JobApplication
    {
        public int Id { get; set; }
        public int CareerId { get; set; }
        public string ApplicantName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ResumeUrl { get; set; }
        public string? CoverLetter { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
