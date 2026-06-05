using Ical.Net;
using Pet.Jira.Application.Extensions.YandexCalendar;
using Pet.Jira.Application.Extensions.YandexCalendar.Dto;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Pet.Jira.Infrastructure.Extensions.YandexCalendar
{
    public class YandexCalDavService : IYandexCalendarService
    {
        private static readonly XNamespace CalDavNs = "urn:ietf:params:xml:ns:caldav";
        private static readonly Regex JiraKeyRegex = new(@"\b[A-Z]+-\d+\b", RegexOptions.Compiled);
        private const string CalDavBaseUrl = "https://caldav.yandex.ru/calendars/{0}/events-default/";

        private readonly HttpClient _http;

        public YandexCalDavService(HttpClient http) => _http = http;

        public async Task<IReadOnlyList<YandexCalendarEventDto>> GetEventsAsync(
            YandexCalendarCredentials credentials,
            DateOnly date,
            CancellationToken ct = default)
        {
            var url = string.Format(CalDavBaseUrl, credentials.Login);
            var body = BuildReportBody(date);

            var request = new HttpRequestMessage(new HttpMethod("REPORT"), url)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/xml")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{credentials.Login}:{credentials.AppPassword}")));
            request.Headers.Add("Depth", "1");

            var response = await _http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var xml = await response.Content.ReadAsStringAsync(ct);
            return ParseCalDavResponse(xml);
        }

        private static string BuildReportBody(DateOnly date)
        {
            var start = date.ToString("yyyyMMdd") + "T000000Z";
            var end = date.AddDays(1).ToString("yyyyMMdd") + "T000000Z";
            return $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<C:calendar-query xmlns:D=""DAV:"" xmlns:C=""urn:ietf:params:xml:ns:caldav"">
  <D:prop><D:getetag/><C:calendar-data/></D:prop>
  <C:filter>
    <C:comp-filter name=""VCALENDAR"">
      <C:comp-filter name=""VEVENT"">
        <C:time-range start=""{start}"" end=""{end}""/>
      </C:comp-filter>
    </C:comp-filter>
  </C:filter>
</C:calendar-query>";
        }

        private static IReadOnlyList<YandexCalendarEventDto> ParseCalDavResponse(string xml)
        {
            var doc = XDocument.Parse(xml);
            var calendarDataElements = doc.Descendants(CalDavNs + "calendar-data");
            var result = new List<YandexCalendarEventDto>();

            foreach (var element in calendarDataElements)
            {
                var icalText = element.Value;
                if (string.IsNullOrWhiteSpace(icalText)) continue;

                var calendar = Calendar.Load(icalText);
                foreach (var vevent in calendar.Events)
                {
                    var start = vevent.DtStart?.AsSystemLocal ?? DateTime.MinValue;
                    var end = vevent.DtEnd?.AsSystemLocal ?? DateTime.MinValue;
                    var summary = vevent.Summary ?? string.Empty;
                    var description = vevent.Description ?? string.Empty;
                    var match = JiraKeyRegex.Match(summary + " " + description);
                    var hint = match.Success ? match.Value : null;

                    result.Add(new YandexCalendarEventDto(summary, start, end, hint));
                }
            }

            return result;
        }
    }
}
