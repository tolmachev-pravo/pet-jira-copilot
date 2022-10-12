using Pet.Jira.Domain.Models.Users;
using TimeZoneConverter;

namespace Pet.Jira.Infrastructure.Jira.Dto
{
    public class UserDto
    {
        public string Username { get; set; }
        public string TimeZoneId { get; set; }

        public User ConvertToUser()
        {
            return new User
            {
                Username = Username,
                TimeZoneInfo = TZConvert.GetTimeZoneInfo(TimeZoneId)
            };
        }
    }
}
