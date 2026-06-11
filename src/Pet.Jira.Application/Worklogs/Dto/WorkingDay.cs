using Pet.Jira.Application.Common.Extensions;
using Pet.Jira.Domain.Models.Worklogs;
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
        public IList<WorkingDayWorklog> Worklogs { get; set; }

        public WorkingDay(
            DateTime date,
            WorkingDaySettings settings,
            IList<WorkingDayWorklog> worklogs = null)
        {
            Date = date;
            Settings = settings;
            Worklogs = worklogs ?? new List<WorkingDayWorklog>();
        }

        /// <summary>
        /// Determines that day is weekend
        /// </summary>
        public bool IsWeekend => Date.DayOfWeek == DayOfWeek.Saturday || Date.DayOfWeek == DayOfWeek.Sunday;

        /// <summary>
        /// Actual worklogs.
        /// Include manual and auto worklogs
        /// </summary>
        public IEnumerable<WorkingDayWorklog> ActualWorklogs => 
            Worklogs.Where(item => item.Type == WorklogType.Actual);

        /// <summary>
        /// Estimated worklogs
        /// </summary>
        public IEnumerable<WorkingDayWorklog> EstimatedWorklogs => 
            Worklogs.Where(item => item.Type == WorklogType.Estimated);

        /// <summary>
        /// Actual worklog time spent
        /// </summary>
        public TimeSpan ActualWorklogTimeSpent => ActualWorklogs.TimeSpent();

        /// <summary>
        /// Estimated worklog time spent
        /// </summary>
        public TimeSpan EstimatedWorklogTimeSpent => EstimatedWorklogs.RemainingTimeSpent();

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

        public int RawEstimatedWorklogCount => EstimatedWorklogs.Count(item => item.RemainingTimeSpent > TimeSpan.Zero);
        public bool HasRawEstimatedWorklogs => RawEstimatedWorklogCount > 0;

        /// <summary>
        /// Time blocked by keyless calendar events that are not yet logged — subtracted from
        /// available day time during distribution. Logged events (matched by exact start/end to an
        /// actual worklog) do not block, since their time is already accounted for.
        /// </summary>
        public TimeSpan CalendarBlockedTime => BlockedCalendarEvents
            .Where(calendarEvent => !IsCalendarEventLogged(calendarEvent))
            .Aggregate(TimeSpan.Zero, (acc, calendarEvent) => acc + calendarEvent.Duration);

        /// <summary>
        /// True when an actual worklog exactly matches the calendar event's start and end.
        /// </summary>
        public bool IsCalendarEventLogged(BlockedCalendarEvent calendarEvent) =>
            ActualWorklogs.Any(worklog =>
                worklog.StartDate == calendarEvent.Start && worklog.CompleteDate == calendarEvent.End);

        /// <summary>
        /// Calendar events without a Jira key — shown for context; they block time but are not loggable.
        /// </summary>
        public IReadOnlyList<BlockedCalendarEvent> BlockedCalendarEvents { get; set; } = new List<BlockedCalendarEvent>();

        public void Refresh()
        {
            WorklogMatching.Match(
                parents: EstimatedWorklogs,
                children: ActualWorklogs);

            // Remaining estimated worklog time spent for logging
            var remainingWorklogTimeSpent = EstimatedWorklogs
                .Where(worklog => worklog.ChildrenTimeSpent == TimeSpan.Zero)
                .Select(worklog => worklog.TimeSpent)
                .Sum();

            // Remaining day time spent for logging, calendar events are treated as hard blocks
            var remainingDayTimeSpent = Settings.WorkingTime - ActualWorklogs.TimeSpent() - CalendarBlockedTime;

            // Fill estimated remaining time spent for each estimated worklog in proportions
            foreach (var estimatedWorklog in EstimatedWorklogs)
            {
                if (remainingWorklogTimeSpent > TimeSpan.Zero
                    && remainingDayTimeSpent > TimeSpan.Zero
                    && estimatedWorklog.ChildrenTimeSpent == TimeSpan.Zero)
                {
                    var percent = estimatedWorklog.TimeSpent / remainingWorklogTimeSpent;
                    var estimatedTimeSpent = percent * remainingDayTimeSpent;
                    estimatedWorklog.UpdateRemainingTimeSpent(estimatedTimeSpent);
                }
                else
                {
                    estimatedWorklog.UpdateRemainingTimeSpent(TimeSpan.Zero);
                }
            }
        }

        public void AddWorklog(WorkingDayWorklog worklog)
        {
            Worklogs.Add(worklog);
            Refresh();
        }
    }
}
