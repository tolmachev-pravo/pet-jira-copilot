using Pet.Jira.Domain.Models.Abstract;
using System;

namespace Pet.Jira.Domain.Models.Users
{
    public class UserProfile : IEntity<string>
    {
        public string Username { get; set; }
        public string TimeZoneId { get; set; }
        public string Avatar { get; set; }
        public bool UseDarkMode { get; set; }
        public string Key => Username;

        public static UserProfile Create()
        {
            return new UserProfile();
        }

        public void UpdateUserInfo(UserProfile userProfile)
        {
            Username = userProfile.Username;
            TimeZoneId = userProfile.TimeZoneId;
            Avatar = userProfile.Avatar;
        }
    }
}
