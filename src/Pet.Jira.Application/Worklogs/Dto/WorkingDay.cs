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

        /// <summary>
        /// Determines that day is weekend
        /// </summary>
        public bool IsWeekend => Date.DayOfWeek == DayOfWeek.Saturday || Date.DayOfWeek == DayOfWeek.Sunday;

        /// <summary>
        /// Actual worklogs
        /// </summary>
        public IEnumerable<WorklogCollectionItem> ActualIWorklogs => 
            Worklogs.Where(item => item.Type == WorklogCollectionItemType.Actual);

        /// <summary>
        /// Estimated worklogs
        /// </summary>
        public IEnumerable<WorklogCollectionItem> EstimatedWorklogs => 
            Worklogs.Where(item => item.Type == WorklogCollectionItemType.Estimated);

        /// <summary>
        /// Actual worklog time spent
        /// </summary>
        public TimeSpan ActualWorklogTimeSpent => ActualIWorklogs.TimeSpent();

        /// <summary>
        /// Estimated remaining worklog time spent
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

        public WorkingDay(
            DateTime date,
            WorkingDaySettings settings,
            IList<WorklogCollectionItem> worklogs = null)
        {
            Date = date;
            Settings = settings;
            Worklogs = worklogs ?? new List<WorklogCollectionItem>();
        }

        public void Refresh()
        {
            foreach (var item in Worklogs)
            {
                item.Refresh(ActualIWorklogs);
            }

            // Автоматические таймлоги
            var autoActualWorklogs = EstimatedWorklogs.SelectMany(record => record.ChildItems);
            // Вручную внесенные таймлоги
            var manualActualWorklogs = ActualIWorklogs.Except(autoActualWorklogs);
            // Вручную списанное время
            var manualTimeSpent = manualActualWorklogs.Sum(record => record.TimeSpent.Ticks);
            // Автоматически списанное время
            var autoTimeSpent = autoActualWorklogs.Sum(record => record.TimeSpent.Ticks);

            // Чистое время выполнения всех незалогированных задач
            var fullRawTimeSpent = EstimatedWorklogs.Where(record => record.ChildTimeSpent == TimeSpan.Zero).Sum(record => record.RawTimeSpent.Ticks);
            // Предполагаемый остаток для автоматического списания времени
            var estimatedRestAutoTimeSpent = Convert.ToDecimal(Settings.WorkingTime.Ticks - manualTimeSpent - autoTimeSpent);

            // Заполняем предполагаемое время для каждой задачи в пропорциях
            foreach (var estimatedWorklog in EstimatedWorklogs)
            {
                if (estimatedWorklog.ChildTimeSpent == TimeSpan.Zero 
                    && estimatedRestAutoTimeSpent > 0
                    && fullRawTimeSpent > 0)
                {
                    var percent = Convert.ToDecimal(estimatedWorklog.RawTimeSpent.Ticks) / fullRawTimeSpent;
                    var estimatedTimeSpent = new TimeSpan(Convert.ToInt64(percent * estimatedRestAutoTimeSpent));
                    estimatedWorklog.TimeSpent = TimeSpan.FromSeconds(Math.Round((estimatedTimeSpent - estimatedWorklog.ChildTimeSpent).TotalSeconds));

                    if (estimatedWorklog.TimeSpent > TimeSpan.Zero
                        && estimatedWorklog.TimeSpent < TimeSpan.FromMinutes(1))
                    {
                        estimatedWorklog.TimeSpent = TimeSpan.FromMinutes(1);
                    }
                }
                else
                {
                    estimatedWorklog.TimeSpent = TimeSpan.Zero;
                }
            }
        }

        public void AddActualItem(WorklogCollectionItem actualItem)
        {
            Worklogs.Add(actualItem);
            Refresh();
        }
    }
}
