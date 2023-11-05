using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Domain.Models.Issues;

namespace Pet.Jira.UnitTests.Application.Extensions
{
	static class IssueExtensions
	{
		public static WorkingDayWorklog CreateWorkingDayWorklog(
			this IIssue issue,
			DateTime date,
			TimeSpan from,
			TimeSpan to)
		{
			return new WorkingDayWorklog
			{
				Issue = issue,
				RawStartDate = date.Add(from),
				RawCompleteDate = date.Add(to)
			};
		}
	}
}
