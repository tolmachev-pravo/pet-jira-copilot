using System;

namespace Pet.Jira.Application.Time
{
    public interface ITimeProvider
    {
        DateTime? ConvertToUserTimezone(DateTime? dateTime, TimeZoneInfo userTimeZone);
        
        DateTime ConvertToUserTimezone(DateTime dateTime, TimeZoneInfo userTimeZone);

        DateTime? ConvertToServerTimezone(DateTime? dateTime, TimeZoneInfo userTimeZone);

        DateTime ConvertToServerTimezone(DateTime dateTime, TimeZoneInfo userTimeZone);
    }
}
