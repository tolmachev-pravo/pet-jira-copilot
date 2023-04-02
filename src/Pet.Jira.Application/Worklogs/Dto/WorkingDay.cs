using Pet.Jira.Application.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Pet.Jira.Application.Worklogs.Dto
{
    public class WorkingDay
    {
        /// <summary>
        /// Date of day without time
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// Settings of working day
        /// </summary>
        [Required]
        public WorkingDaySettings Settings { get; set; }

        /// <summary>
        /// Worklog items
        /// </summary>
        public IList<WorklogCollectionItem> Worklogs { get; set; }

        public WorkingDay(
            DateTime date,
            WorkingDaySettings settings,
            IList<WorklogCollectionItem> worklogs = null)
        {
            Date = date;
            Settings = settings;
            Worklogs = worklogs ?? new List<WorklogCollectionItem>();
        }

        /// <summary>
        /// Determines that day is weekend
        /// </summary>
        public bool IsWeekend => Date.DayOfWeek == DayOfWeek.Saturday || Date.DayOfWeek == DayOfWeek.Sunday;

        /// <summary>
        /// Actual worklogs.
        /// Include manual and auto worklogs
        /// </summary>
        public IEnumerable<WorklogCollectionItem> ActualWorklogs => 
            Worklogs.Where(item => item.Type == WorklogCollectionItemType.Actual);

        /// <summary>
        /// Estimated worklogs
        /// </summary>
        public IEnumerable<WorklogCollectionItem> EstimatedWorklogs => 
            Worklogs.Where(item => item.Type == WorklogCollectionItemType.Estimated);

        /// <summary>
        /// Actual worklog time spent
        /// </summary>
        public TimeSpan ActualWorklogTimeSpent => ActualWorklogs.TimeSpent();

        /// <summary>
        /// Estimated worklog time spent
        /// </summary>
        public TimeSpan EstimatedWorklogTimeSpent => EstimatedWorklogs.TimeSpent();

        /// <summary>
        /// Worklog time spent
        /// </summary>
        public TimeSpan WorklogTimeSpent => ActualWorklogTimeSpent + EstimatedWorklogTimeSpent;

        /// <summary>
        /// Percent of progress time spent by day
        /// </summary>
        public int Progress => WorklogTimeSpent > TimeSpan.Zero
            ? Convert.ToInt32(ActualWorklogTimeSpent * 100 / WorklogTimeSpent)
            : 0;

        public int RawEstimatedWorklogCount => EstimatedWorklogs.Count(item => item.TimeSpent > TimeSpan.Zero);
        public bool HasRawEstimatedWorklogs => RawEstimatedWorklogCount > 0;

        public void Refresh()
        {
            foreach (var item in Worklogs)
            {
                item.AttachSuitableChildren(ActualWorklogs);
            }

            // Remaining raw write-off time spent for unlogged worklogs
            var remainingRawTimeSpent = EstimatedWorklogs
                .Where(worklog => worklog.ChildrenTimeSpent == TimeSpan.Zero)
                .Select(worklog => worklog.RawTimeSpent)
                .Sum();

            // Remaining time spent for logging
            var remainingTimeSpent = Settings.WorkingTime - ActualWorklogs.TimeSpent();

            // Fill estimated remaining time spent for each estimated worklog in proportions
            foreach (var estimatedWorklog in EstimatedWorklogs)
            {
                if (remainingRawTimeSpent > TimeSpan.Zero
                    && remainingTimeSpent > TimeSpan.Zero
                    && estimatedWorklog.ChildrenTimeSpent == TimeSpan.Zero)
                {
                    var percent = estimatedWorklog.RawTimeSpent / remainingRawTimeSpent;
                    var estimatedTimeSpent = percent * remainingTimeSpent;
                    estimatedWorklog.UpdateTimeSpent(estimatedTimeSpent);
                }
                else
                {
                    estimatedWorklog.UpdateTimeSpent(TimeSpan.Zero);
                }
            }
        }

        public void AddWorklog(WorklogCollectionItem worklog)
        {
            Worklogs.Add(worklog);
            Refresh();
        }
    }
}
