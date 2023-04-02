using Pet.Jira.Application.Extensions;
using Pet.Jira.Domain.Models.Issues;
using Pet.Jira.Domain.Models.Worklogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Pet.Jira.Application.Worklogs.Dto
{
    public class WorklogCollectionItem : IHasTimeSpent
    {
        public DateTime StartDate { get; set; }
        public DateTime CompleteDate { get; set; }
        public TimeSpan TimeSpent { get; set; }

        /// <summary>
        /// Issue
        /// </summary>
        [Required]
        public IIssue Issue { get; set; }

        public WorklogCollectionItemType Type { get; set; }
        public WorklogCollectionItemSource Source { get; set; }

        public IList<WorklogCollectionItem> Children { get; set; }
        public WorklogCollectionItem Parent { get; set; }

        public TimeSpan ChildrenTimeSpent => Children.TimeSpent();
        public bool IsEmpty => TimeSpent == TimeSpan.Zero;
        public TimeSpan RawTimeSpent => CompleteDate - StartDate;

        public WorklogCollectionItem()
        {
            Children = new List<WorklogCollectionItem>();
        }

        /// <summary>
        /// Update time spent
        /// </summary>
        /// <param name="timeSpan"></param>
        public void UpdateTimeSpent(TimeSpan timeSpan)
        {
            if (timeSpan > TimeSpan.Zero
                && timeSpan < TimeSpan.FromMinutes(1))
            {
                timeSpan = TimeSpan.FromMinutes(1);
            }

            TimeSpent = timeSpan;
        }

        public static WorklogCollectionItem Create(
            IWorklog worklog,
            WorklogCollectionItemType type,
            IEnumerable<WorklogCollectionItem> dailyItems = null)
        {
            var result = new WorklogCollectionItem
            {
                StartDate = worklog.StartDate,
                CompleteDate = worklog.CompleteDate,
                Issue = worklog.Issue,
                Type = type
            };

            switch (worklog.Source)
            {
                case WorklogSource.Assignee:
                    result.Source = WorklogCollectionItemSource.Assignee;
                    break;
                case WorklogSource.Comment:
                    result.Source = WorklogCollectionItemSource.Comment;
                    break;
            }

            result.UpdateTimeSpent(worklog.TimeSpent);
            result.AttachSuitableChildren(dailyItems);

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
                Type = type,
                Source = Source
            };
        }

        /// <summary>
        /// Attach suitable children worklogs to current worklog
        /// </summary>
        /// <param name="applicants"></param>
        public void AttachSuitableChildren(IEnumerable<WorklogCollectionItem> applicants)
        {
            if (!applicants.IsEmpty())
            {
                Children = applicants
                    .Where(applicant =>
                        applicant.Issue.Key == this.Issue.Key
                        && applicant.StartDate == this.CompleteDate)
                    .ToList();
                foreach (var child in Children)
                {
                    child.Parent = this;
                }
            }
        }
    }
}
