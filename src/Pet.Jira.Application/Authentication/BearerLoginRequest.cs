using System.ComponentModel.DataAnnotations;

namespace Pet.Jira.Application.Authentication
{
    public class BearerLoginRequest
    {
        [Required]
        public string Token { get; set; }
    }
}
