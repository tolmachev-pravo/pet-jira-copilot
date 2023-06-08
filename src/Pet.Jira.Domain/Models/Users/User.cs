using Pet.Jira.Domain.Models.Abstract;

namespace Pet.Jira.Domain.Models.Users
{
    public class User : IEntity<string>
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string PersonalAccessToken { get; set; }

        public string Key => Username;
    }
}
