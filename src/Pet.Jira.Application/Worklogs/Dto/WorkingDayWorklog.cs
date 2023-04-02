using Pet.Jira.Application.Extensions;
using Pet.Jira.Domain.Models.Issues;
using Pet.Jira.Domain.Models.Worklogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Pet.Jira.Application.Worklogs.Dto
{
    public class WorkingDayWorklog : IHasTimeSpent
    {
        public DateTime StartDate { get; set; }
        public DateTime CompleteDate { get; set; }
        public TimeSpan TimeSpent { get; set; }

        /// <summary>
        /// Issue
        /// </summary>
        [Required]
        public IIssue Issue { get; set; }

        public WorklogType Type { get; set; }
        public WorklogSource Source { get; set; }

        public IList<WorkingDayWorklog> Children { get; set; }
        public WorkingDayWorklog Parent { get; set; }

        public TimeSpan ChildrenTimeSpent => Children.TimeSpent();
        public bool IsEmpty => TimeSpent == TimeSpan.Zero;
        public TimeSpan RawTimeSpent => CompleteDate - StartDate;

        public WorkingDayWorklog()
        {
            Children = new List<WorkingDayWorklog>();
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

        public static WorkingDayWorklog Create(
            IWorklog worklog,
            WorklogType type,
            IEnumerable<WorkingDayWorklog> dailyItems = null)
        {
            var result = new WorkingDayWorklog
            {
                StartDate = worklog.StartDate,
                CompleteDate = worklog.CompleteDate,
                Issue = worklog.Issue,
                Type = type,
                Source = worklog.Source
            };

            result.UpdateTimeSpent(worklog.TimeSpent);
            result.AttachSuitableChildren(dailyItems);

            return result;
        }

        public WorkingDayWorklog Clone(WorklogType type)
        {
            return new WorkingDayWorklog
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
        public void AttachSuitableChildren(IEnumerable<WorkingDayWorklog> applicants)
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
