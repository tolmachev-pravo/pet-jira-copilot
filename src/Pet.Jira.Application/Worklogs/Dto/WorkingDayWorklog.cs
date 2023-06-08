using Pet.Jira.Application.Extensions;
using Pet.Jira.Domain.Models.Issues;
using Pet.Jira.Domain.Models.Worklogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Pet.Jira.Application.Worklogs.Dto
{
    public class WorkingDayWorklog
    {
        public DateTime RawStartDate { get; set; }
        public DateTime RawCompleteDate { get; set; }
        public TimeSpan RawTimeSpent => RawCompleteDate - RawStartDate;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime CompleteDate { get; set; }

        public TimeSpan TimeSpent => CompleteDate - StartDate;

        public TimeSpan RemainingTimeSpent { get; set; }

        /// <summary>
        /// Issue
        /// </summary>
        [Required]
        public IIssue Issue { get; set; }

        [Required]
        public WorklogType Type { get; set; }

        [Required]
        public WorklogSource Source { get; set; }

        public WorkingDay WorkingDay { get; set; }

        public IWorklog Worklog { get; set; }

        public IList<WorkingDayWorklog> Children { get; set; }
        public WorkingDayWorklog Parent { get; set; }

        public TimeSpan ChildrenTimeSpent => Children.TimeSpent();
        public bool IsEmpty => RemainingTimeSpent == TimeSpan.Zero;

        public WorkingDayWorklog()
        {
            Children = new List<WorkingDayWorklog>();
        }

        public WorkingDayWorklog(
            DateTime startDate,
            DateTime completeDate,
            IIssue issue,
            WorklogType type,
            WorklogSource source)
        {
            RawStartDate = startDate;
            RawCompleteDate = completeDate;
            Issue = issue;
            Type = type;
            Source = source;
            Children = new List<WorkingDayWorklog>();
        }

        /// <summary>
        /// Update remaining time spent
        /// </summary>
        /// <param name="timeSpan"></param>
        public void UpdateRemainingTimeSpent(TimeSpan timeSpan)
        {
            if (timeSpan > TimeSpan.Zero
                && timeSpan < TimeSpan.FromMinutes(1))
            {
                timeSpan = TimeSpan.FromMinutes(1);
            }

            RemainingTimeSpent = timeSpan.Round();
        }

        public static WorkingDayWorklog CreateActual(
            IWorklog worklog)
        {
            var result = new WorkingDayWorklog
            {
                RawStartDate = worklog.StartDate,
                RawCompleteDate = worklog.CompleteDate,
                StartDate = worklog.StartDate,
                CompleteDate = worklog.CompleteDate,
                Issue = worklog.Issue,
                Type = WorklogType.Actual,
                Source = worklog.Source
            };

            result.UpdateRemainingTimeSpent(worklog.TimeSpent);

            return result;
        }

        public static WorkingDayWorklog CreateEstimated(
            IWorklog worklog,
            DateTime day,
            TimeSpan dailyWorkingStartTime,
            TimeSpan dailyWorkingEndTime,
            IEnumerable<WorkingDayWorklog> dailyActualWorklogs = null)
        {
            var startOfWorkingDay = day.Add(dailyWorkingStartTime);
            var endOfWorkingDay = day.Add(dailyWorkingEndTime);

            DateTime startDate = AdaptWorkingTime(worklog.StartDate, startOfWorkingDay, endOfWorkingDay);
            DateTime completeDate = AdaptWorkingTime(worklog.CompleteDate, startOfWorkingDay, endOfWorkingDay);

            var result = new WorkingDayWorklog
            {
                RawStartDate = worklog.StartDate,
                RawCompleteDate = worklog.CompleteDate,
                StartDate = startDate,
                CompleteDate = completeDate,
                Issue = worklog.Issue,
                Type = WorklogType.Estimated,
                Source = worklog.Source
            };

            result.UpdateRemainingTimeSpent(result.TimeSpent);
            result.AttachSuitableChildren(dailyActualWorklogs);

            return result;
        }        

        public WorkingDayWorklog Clone(WorklogType type)
        {
            return new WorkingDayWorklog
            {
                StartDate = CompleteDate,
                CompleteDate = CompleteDate.Add(RemainingTimeSpent),
                RemainingTimeSpent = RemainingTimeSpent,
                Issue = Issue,
                Type = type,
                Source = Source
            };
        }

        private static DateTime AdaptWorkingTime(
            DateTime value,
            DateTime startOfWorkingDay,
            DateTime endOfWorkingDay)
        {
            if (value > endOfWorkingDay)
            {
                return endOfWorkingDay;
            }
            else if (value < startOfWorkingDay)
            {
                return startOfWorkingDay;
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// Attach suitable children worklogs to current worklog
        /// </summary>
        /// <param name="dailyActualWorklogs"></param>
        public void AttachSuitableChildren(IEnumerable<WorkingDayWorklog> dailyActualWorklogs)
        {
            if (!dailyActualWorklogs.IsEmpty())
            {
                Children = dailyActualWorklogs
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
