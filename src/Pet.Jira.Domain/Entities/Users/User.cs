using Pet.Jira.Domain.Entities.Integrations;
using System.Collections.Generic;

namespace Pet.Jira.Domain.Entities.Users
{
	public class User : BaseEntity
	{
		public string Username { get; set; }

		public ICollection<UserCalendarConnection> CalendarConnections { get; set; } = new List<UserCalendarConnection>();
	}
}
