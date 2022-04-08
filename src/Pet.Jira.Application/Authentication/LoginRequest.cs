using System.ComponentModel.DataAnnotations;

namespace Pet.Jira.Application.Authentication
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
