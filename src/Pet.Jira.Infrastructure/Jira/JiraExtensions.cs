using Pet.Jira.Domain.Models.Worklogs;
using Pet.Jira.Infrastructure.Jira.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pet.Jira.Infrastructure.Jira
{
    public static class JiraExtensions
    {
        public static IEnumerable<T> ConvertTo<T>(this IList<IssueChangeLogItemDto> issueChangeLogItems)
            where T: IWorklog, new()
        {
            var i = 0;
            while (i < issueChangeLogItems.Count())
            {
                var item = issueChangeLogItems[i];
                // 1. Первый элемент сразу выходит из прогресса. Значит это завершающий
                if (item.FromInProgress)
                {
                    yield return new T()
                    {
                        CompletedAt = item.ChangeLog.CreatedDate,
                        StartedAt = DateTime.MinValue,
                        Issue = item.ChangeLog.Issue.Adapt()
                    };
                }
                // 2. Это последний элемент и он не завершается
                else if (i == (issueChangeLogItems.Count() - 1))
                {
                    yield return new T()
                    {
                        CompletedAt = DateTime.MaxValue,
                        StartedAt = item.ChangeLog.CreatedDate,
                        Issue = item.ChangeLog.Issue.Adapt()
                    };
                }
                // 3. Обычный случай когда после FromInProgress следует ToInProgress
                else
                {
                    yield return new T()
                    {
                        CompletedAt = issueChangeLogItems[i + 1].ChangeLog.CreatedDate,
                        StartedAt = item.ChangeLog.CreatedDate,
                        Issue = item.ChangeLog.Issue.Adapt()
                    };
                }

                i += 2;
            }
        }
    }
}
