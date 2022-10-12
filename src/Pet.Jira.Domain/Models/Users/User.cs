using System;

namespace Pet.Jira.Domain.Models.Users
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public TimeZoneInfo TimeZoneInfo { get; set; }
    }
}
