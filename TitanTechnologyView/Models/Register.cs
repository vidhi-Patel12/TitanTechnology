using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TitanTechnologyView.Models
{
    public class Register
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(100)]
        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        [Required, EmailAddress, MaxLength(150)]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6), MaxLength(255)]
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        // NOTE: API expects snake_case "contact_number" — map explicitly
        [MaxLength(20)]
        [JsonPropertyName("contact_number")]
        public string? contact_number { get; set; }

        [Required]
        [JsonPropertyName("userRoleId")]
        public int UserRoleId { get; set; }

    }
}
