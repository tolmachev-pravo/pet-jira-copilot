using Pet.Jira.Application.Common.Mapping;
using Pet.Jira.Domain.Entities.Integrations;
using System;

namespace Pet.Jira.Application.Integrations.Dto
{
	public class UserCalendarConnectionDto : IMapFrom<UserCalendarConnection>
	{
		public CalendarProvider Provider { get; set; }
		public UserCalendarConnectionStatus Status { get; set; }
		public DateTime? ExpiresAtUtc { get; set; }
		public string Scope { get; set; }
		public string ExternalAccountId { get; set; }
		public string ExternalLogin { get; set; }
		public DateTime? LastConnectedAtUtc { get; set; }
		public DateTime? LastRefreshAtUtc { get; set; }
		public string LastError { get; set; }
	}
}
