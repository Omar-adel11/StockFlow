using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Team
{
    public class SendInviteRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;
    }
}