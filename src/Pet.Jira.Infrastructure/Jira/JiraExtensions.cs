using Pet.Jira.Domain.Models.Worklogs;
using Pet.Jira.Infrastructure.Jira.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pet.Jira.Infrastructure.Jira
{
    public static class JiraExtensions
    {
        public static IEnumerable<T> ConvertTo<T>(this IList<IssueChangeLogItemDto> issueChangeLogItems, string issueStatusId)
            where T: IWorklog, new()
        {
            var i = 0;
            while (i < issueChangeLogItems.Count())
            {
                var item = issueChangeLogItems[i];
                // 1. Первый элемент сразу выходит из прогресса. Значит это завершающий
                if (item.FromId == issueStatusId)
                {
                    yield return new T()
                    {
                        CompleteDate = item.ChangeLog.CreatedDate,
                        StartDate = DateTime.MinValue,
                        Issue = item.ChangeLog.Issue.Adapt(),
                        Author = item.Author
                    };
                }
                // 2. Это последний элемент и он не завершается
                else if (i == (issueChangeLogItems.Count() - 1))
                {
                    yield return new T()
                    {
                        CompleteDate = DateTime.MaxValue,
                        StartDate = item.ChangeLog.CreatedDate,
                        Issue = item.ChangeLog.Issue.Adapt(),
                        Author = item.Author
                    };
                }
                // 3. Обычный случай когда после FromInProgress следует ToInProgress
                else
                {
                    yield return new T()
                    {
                        CompleteDate = issueChangeLogItems[i + 1].ChangeLog.CreatedDate,
                        StartDate = item.ChangeLog.CreatedDate,
                        Issue = item.ChangeLog.Issue.Adapt(),
                        Author = item.Author
                    };
                }

                i += 2;
            }
        }
    }
}
