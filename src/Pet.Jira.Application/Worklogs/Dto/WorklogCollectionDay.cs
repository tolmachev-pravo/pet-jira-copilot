using System;
using System.Collections.Generic;
using System.Linq;

namespace Pet.Jira.Application.Worklogs.Dto
{
    public class WorklogCollectionDay
    {
        public DateTime Date { get; set; }
        public IList<WorklogCollectionItem> Items { get; set; } = new List<WorklogCollectionItem>();

        public TimeSpan DailyWorkingStartTime { get; set; }
        public TimeSpan DailyWorkingEndTime { get; set; }

        public IEnumerable<WorklogCollectionItem> ActualItems => Items.Where(item => item.Type == WorklogCollectionItemType.Actual);
        public IEnumerable<WorklogCollectionItem> EstimatedItems => Items.Where(item => item.Type == WorklogCollectionItemType.Estimated);

        public TimeSpan ActualWorklogsSum => new TimeSpan(ActualItems?.Sum(item => item.TimeSpent.Ticks) ?? 0);
        public TimeSpan EstimatedWorklogsSum => new TimeSpan(EstimatedItems?.Sum(item => item.TimeSpent.Ticks) ?? 0);

        public TimeSpan CalculatedWorklogsSum =>
            ActualWorklogsSum + new TimeSpan(EstimatedItems?.Sum(item => item.TimeSpent.Ticks) ?? 0);

        public int Progress => CalculatedWorklogsSum > TimeSpan.Zero
            ? Convert.ToInt32(ActualWorklogsSum * 100 / CalculatedWorklogsSum)
            : 0;

        public int RawEstimatedWorklogCount => EstimatedItems.Count(item => item.TimeSpent > TimeSpan.Zero);
        public bool HasRawEstimatedWorklogs => RawEstimatedWorklogCount > 0;

        public void Refresh()
        {
            foreach (var item in Items)
            {
                item.Refresh(ActualItems);
            }

            var workTime = DailyWorkingEndTime - DailyWorkingStartTime - TimeSpan.FromHours(1);
            // Время зафиксированное за день
            var dayTimeSpent = new TimeSpan(ActualItems.Sum(record => record.TimeSpent.Ticks));
            // Автоматические
            var autoActualWorklogs = EstimatedItems.SelectMany(record => record.ChildItems);
            // Вручную внесенные таймлоги
            var manualActualWorklogs = ActualItems.Except(autoActualWorklogs);
            // Вручную списанное время
            var manualTimeSpent = manualActualWorklogs.Sum(record => record.TimeSpent.Ticks);

            // Время выполнения всех задач
            var fullRawTimeSpent = EstimatedItems.Sum(record => record.TimeSpent.Ticks);
            // Предполагаемый остаток для автоматического списания времени
            var estimatedRestAutoTimeSpent = Convert.ToDecimal(workTime.Ticks - manualTimeSpent);

            // Заполняем предполагаемое время для каждой задачи в пропорциях
            foreach (var estimatedWorklog in EstimatedItems)
            {
                if (estimatedWorklog.ChildTimeSpent == TimeSpan.Zero)
                {
                    var percent = Convert.ToDecimal(estimatedWorklog.TimeSpent.Ticks) / fullRawTimeSpent;
                    var estimatedTimeSpent = new TimeSpan(Convert.ToInt64(percent * estimatedRestAutoTimeSpent));
                    estimatedWorklog.TimeSpent = TimeSpan.FromSeconds(Math.Round((estimatedTimeSpent - estimatedWorklog.ChildTimeSpent).TotalSeconds));
                }
                else
                {
                    estimatedWorklog.TimeSpent = TimeSpan.Zero;
                }
            }
        }

        public void AddActualItem(WorklogCollectionItem actualItem)
        {
            Items.Add(actualItem);
            Refresh();
        }
    }
}
