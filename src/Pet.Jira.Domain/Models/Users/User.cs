using Pet.Jira.Domain.Models.Abstract;
using System;

namespace Pet.Jira.Domain.Models.Users
{
    public class User : IEntity<string>
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public TimeZoneInfo TimeZoneInfo { get; set; }
        public string Key => Username;
    }
}
