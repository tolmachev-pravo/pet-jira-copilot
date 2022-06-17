using Pet.Jira.Domain.Models.Issues;
using Pet.Jira.Domain.Models.Worklogs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pet.Jira.Application.Worklogs.Dto
{
    public class WorklogCollectionItem
    {
        public DateTime StartDate { get; set; }
        public DateTime CompleteDate { get; set; }
        public TimeSpan TimeSpent { get; set; }
        public IIssue Issue { get; set; }

        public WorklogCollectionItemType Type { get; set; }

        public IList<WorklogCollectionItem> ChildItems{ get; set; }
        public TimeSpan ChildTimeSpent => new TimeSpan(ChildItems.Sum(item => item.TimeSpent.Ticks));
        public WorklogCollectionItem ParentItem { get; set; }
        public bool IsEmpty => TimeSpent == TimeSpan.Zero;
        public TimeSpan RawTimeSpent => CompleteDate - StartDate;

        public static WorklogCollectionItem Create(
            IWorklog worklog,
            WorklogCollectionItemType type,
            IEnumerable<WorklogCollectionItem> dailyItems = null)
        {
            var result = new WorklogCollectionItem
            {
                StartDate = worklog.StartDate,
                CompleteDate = worklog.CompleteDate,
                TimeSpent = worklog.TimeSpent,
                Issue = worklog.Issue,
                Type = type
            };
            if (dailyItems != null)
            {
                result.ChildItems = dailyItems
                    .Where(item => item.Issue.Key == result.Issue.Key
                                   && item.StartDate == result.CompleteDate)
                    .ToList();
            }

            return result;
        }

        public WorklogCollectionItem Clone(WorklogCollectionItemType type)
        {
            return new WorklogCollectionItem
            {
                StartDate = CompleteDate,
                CompleteDate = CompleteDate.Add(TimeSpent),
                TimeSpent = TimeSpent,
                Issue = Issue,
                Type = type
            };
        }

        public void Refresh(IEnumerable<WorklogCollectionItem> dailyItems = null)
        {
            if (dailyItems != null)
            {
                ChildItems = dailyItems
                    .Where(item => item.Issue.Key == Issue.Key
                                   && item.StartDate == CompleteDate)
                    .ToList();
                foreach (var item in ChildItems)
                {
                    item.ParentItem = this;
                }
            }
        }
    }
}
