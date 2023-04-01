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

        public IEnumerable<WorklogCollectionItem> ActualItems => Worklogs.Where(item => item.Type == WorklogCollectionItemType.Actual);
        public IEnumerable<WorklogCollectionItem> EstimatedItems => Worklogs.Where(item => item.Type == WorklogCollectionItemType.Estimated);
        public TimeSpan ActualWorklogsSum => new TimeSpan(ActualItems?.Sum(item => item.TimeSpent.Ticks) ?? 0);
        public TimeSpan EstimatedWorklogsSum => new TimeSpan(EstimatedItems?.Sum(item => item.TimeSpent.Ticks) ?? 0);
        public TimeSpan CalculatedWorklogsSum => ActualWorklogsSum + EstimatedWorklogsSum;

        public int Progress => CalculatedWorklogsSum > TimeSpan.Zero
            ? Convert.ToInt32(ActualWorklogsSum * 100 / CalculatedWorklogsSum)
            : 0;

        public int RawEstimatedWorklogCount => EstimatedItems.Count(item => item.TimeSpent > TimeSpan.Zero);
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
                item.Refresh(ActualItems);
            }

            // Время зафиксированное за день
            var dayTimeSpent = new TimeSpan(ActualItems.Sum(record => record.TimeSpent.Ticks));
            // Автоматические
            var autoActualWorklogs = EstimatedItems.SelectMany(record => record.ChildItems);
            // Вручную внесенные таймлоги
            var manualActualWorklogs = ActualItems.Except(autoActualWorklogs);
            // Вручную списанное время
            var manualTimeSpent = manualActualWorklogs.Sum(record => record.TimeSpent.Ticks);
            // Автоматически списанное время
            var autoTimeSpent = autoActualWorklogs.Sum(record => record.TimeSpent.Ticks);

            // Чистое время выполнения всех незалогированных задач
            var fullRawTimeSpent = EstimatedItems.Where(record => record.ChildTimeSpent == TimeSpan.Zero).Sum(record => record.RawTimeSpent.Ticks);
            // Предполагаемый остаток для автоматического списания времени
            var estimatedRestAutoTimeSpent = Convert.ToDecimal(Settings.WorkingTime.Ticks - manualTimeSpent - autoTimeSpent);

            // Заполняем предполагаемое время для каждой задачи в пропорциях
            foreach (var estimatedWorklog in EstimatedItems)
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
