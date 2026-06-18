using Pet.Jira.Application.Time;
using Pet.Jira.Domain.Models.Worklogs;
using Pet.Jira.Infrastructure.Jira.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pet.Jira.Infrastructure.Jira
{
    public static class JiraExtensions
    {
        public static bool IsJiraKey(this string input)
        {
            var pattern = "^[A-Z][A-Z0-9]+-[0-9]+$";
			var regex = new Regex(pattern);
			var match = regex.Match(input);
            return match.Success;
		}

        public static IEnumerable<T> ConvertTo<T>(this IList<IssueChangeLogItemDto> issueChangeLogItems,
            string issueStatusId,
            ITimeProvider timeProvider,
            TimeZoneInfo timeZoneInfo,
			WorklogSource worklogSource)
            where T : IWorklog, new()
        {
            return issueChangeLogItems
                .ToStatusIntervals(issueStatusId, timeProvider, timeZoneInfo)
                .Select(interval => new T
                {
                    StartDate = interval.Start,
                    CompleteDate = interval.End,
                    Issue = interval.Issue.Adapt(),
                    Author = interval.Author,
                    Source = worklogSource
                });
        }

        /// <summary>
        /// Pairs ordered status-change changelog items into the time intervals an
        /// issue spent in <paramref name="issueStatusId"/>. Worklog-agnostic core
        /// shared by the worklog and event pipelines.
        /// </summary>
        public static IEnumerable<StatusInterval> ToStatusIntervals(this IList<IssueChangeLogItemDto> issueChangeLogItems,
            string issueStatusId,
            ITimeProvider timeProvider,
            TimeZoneInfo timeZoneInfo)
        {
            var i = 0;
            while (i < issueChangeLogItems.Count)
            {
                var item = issueChangeLogItems[i];
                // 1. Первый элемент сразу выходит из прогресса. Значит это завершающий
                if (item.FromId == issueStatusId)
                {
                    yield return new StatusInterval(
                        Start: DateTime.MinValue,
                        End: timeProvider.ConvertToUserTimezone(item.ChangeLog.CreatedDate, timeZoneInfo),
                        Author: item.Author,
                        Issue: item.ChangeLog.Issue);
                }
                // 2. Это последний элемент и он не завершается
                else if (i == (issueChangeLogItems.Count - 1))
                {
                    yield return new StatusInterval(
                        Start: timeProvider.ConvertToUserTimezone(item.ChangeLog.CreatedDate, timeZoneInfo),
                        End: DateTime.MaxValue,
                        Author: item.Author,
                        Issue: item.ChangeLog.Issue);
                }
                // 3. Обычный случай когда после FromInProgress следует ToInProgress
                else
                {
                    yield return new StatusInterval(
                        Start: timeProvider.ConvertToUserTimezone(item.ChangeLog.CreatedDate, timeZoneInfo),
                        End: timeProvider.ConvertToUserTimezone(issueChangeLogItems[i + 1].ChangeLog.CreatedDate, timeZoneInfo),
                        Author: item.Author,
                        Issue: item.ChangeLog.Issue);
                }

                i += 2;
            }
        }

        public static IEnumerable<T> ConvertTo<T>(
            this List<IssueCommentDto> comments,
            ITimeProvider timeProvider,
            TimeZoneInfo timeZoneInfo,
            WorklogSource source,
            TimeSpan time)
                where T : IWorklog, new()
        {
            foreach (var comment in comments)
            {
                var createdDate = timeProvider.ConvertToUserTimezone(comment.CreatedDate, timeZoneInfo);
                yield return new T()
                {
                    CompleteDate = createdDate,
                    StartDate = createdDate.Add(-time),
                    Issue = comment.Issue.Adapt(),
                    Author = comment.Author,
                    Source = source
                };
            }
        }
    }
}
