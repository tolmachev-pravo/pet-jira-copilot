using MediatR;
using Pet.Jira.Domain.Models.Worklogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Pet.Jira.Application.Extensions;
using Pet.Jira.Application.Worklogs.Dto;

namespace Pet.Jira.Application.Worklogs.Queries
{
    public class GetDailyWorklogSummaries
    {
        public class Query : IRequest<Model>
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public TimeSpan DailyWorkingStartTime { get; set; }
            public TimeSpan DailyWorkingEndTime { get; set; }
            public string IssueStatusId { get; set; }
        }

        public class Model
        {
            public WorklogCollection WorklogCollection { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, Model>
        {
            private readonly IWorklogDataSource _worklogDataSource;

            public QueryHandler(IWorklogDataSource worklogDataSource)
            {
                _worklogDataSource = worklogDataSource;
            }

            public async Task<Model> Handle(
                Query query,
                CancellationToken cancellationToken)
            {
                var worklogCollection = await CalculateWorklogCollection(query);
                return new Model { WorklogCollection = worklogCollection };
            }

            private async Task<WorklogCollection> CalculateWorklogCollection(Query query)
            {
                var rawIssueWorklogs = await _worklogDataSource.GetRawIssueWorklogsAsync(
                    new GetRawIssueWorklogs.Query()
                    {
                        StartDate = query.StartDate,
                        EndDate = query.EndDate,
                        IssueStatusId = query.IssueStatusId
                    });

                var issueWorklogs = await _worklogDataSource.GetIssueWorklogsAsync(
                    new GetIssueWorklogs.Query()
                    {
                        StartDate = query.StartDate,
                        EndDate = query.EndDate
                    });

                var days = CalculateDaily(issueWorklogs, rawIssueWorklogs, query);
                foreach (var day in days)
                {
                    Recalculate(day);
                }

                return new WorklogCollection() { Days = days.ToList() };
            }

            public async Task<IList<DailyWorklogSummary>> GetUserDayWorklogs(Query query)
            {
                try
                {
                    var rawIssueWorklogs = await _worklogDataSource.GetRawIssueWorklogsAsync(
                        new GetRawIssueWorklogs.Query()
                        {
                            StartDate = query.StartDate,
                            EndDate = query.EndDate,
                            IssueStatusId = query.IssueStatusId
                        });

                    var dailyRawIssueWorklogs = rawIssueWorklogs.SplitByDays(
                        firstDate: query.StartDate,
                        lastDate: query.EndDate,
                        dailyWorkingStartTime: query.DailyWorkingStartTime,
                        dailyWorkingEndTime: query.DailyWorkingEndTime);

                    var rawEstimatedWorklogs = rawIssueWorklogs.Select(item => new EstimatedWorklog
                    {
                        CompletedAt = item.CompletedAt,
                        StartedAt = item.StartedAt,
                        Issue = item.Issue,
                    });
                    var estimatedWorklogs = PrepareEstimatedWorklogs(rawEstimatedWorklogs, query);
                    var issueWorklogs = await _worklogDataSource.GetIssueWorklogsAsync(new GetIssueWorklogs.Query()
                    {
                        StartDate = query.StartDate,
                        EndDate = query.EndDate
                    });
                    var actualWorklogs = issueWorklogs.Select(issueWorklog => ActualWorklog.Create(issueWorklog));

                    var result = new List<DailyWorklogSummary>();
                    var cycleDate = query.EndDate.Date;
                    while (cycleDate >= query.StartDate.Date)
                    {
                        result.Add(new DailyWorklogSummary
                        {
                            Date = cycleDate,
                            ActualWorklogs = actualWorklogs.Where(record => record.StartedAt.Date == cycleDate).ToList(),
                            EstimatedWorklogs = estimatedWorklogs.Where(record => record.StartedAt.Date == cycleDate).ToList()
                        });
                        cycleDate = cycleDate.AddDays(-1);
                    }

                    Calculate(result, query);
                    return result;
                }
                catch (AuthenticationException e)
                {
                    throw new Exception($"Authentication exception") { Source = e.Source };
                }
            }

            private IEnumerable<WorklogCollectionDay> CalculateDaily(
                IEnumerable<IWorklog> issueWorklogs,
                IEnumerable<IWorklog> rawIssueWorklogs,
                Query query)
            {
                var day = query.EndDate.Date;
                var splitedRawIssueWorklogs = rawIssueWorklogs.SplitByDays(
                    firstDate: query.StartDate,
                    lastDate: query.EndDate,
                    dailyWorkingStartTime: query.DailyWorkingStartTime,
                    dailyWorkingEndTime: query.DailyWorkingEndTime);

                while (day >= query.StartDate.Date)
                {
                    var dailyIssueWorklogs = issueWorklogs
                        .Where(worklog => worklog.StartedAt.Date == day)
                        .Select(worklog => WorklogCollectionItem.Create(worklog, WorklogCollectionItemType.Actual));

                    var dailyRawIssueWorklogs = splitedRawIssueWorklogs
                        .Where(worklog => worklog.StartedAt.Date == day)
                        .Select(worklog => WorklogCollectionItem.Create(worklog, WorklogCollectionItemType.Estimated, dailyIssueWorklogs));

                    yield return new WorklogCollectionDay
                    {
                        Date = day,
                        Items = dailyIssueWorklogs.Union(dailyRawIssueWorklogs).ToList()
                    };
                    day = day.AddDays(-1);
                }
            }

            private void Recalculate(WorklogCollectionDay list)
            {
                var workTime = TimeSpan.FromHours(8);
                // Время зафиксированное за день
                var dayTimeSpent = new TimeSpan(list.ActualItems.Sum(record => record.TimeSpent.Ticks));

                // Автоматические
                var autoActualWorklogs = list.EstimatedItems.SelectMany(record => record.ChildItems);
                // Вручную внесенные таймлоги
                var manualActualWorklogs = list.ActualItems.Except(autoActualWorklogs);
                // Вручную списанное время
                var manualTimeSpent = manualActualWorklogs.Sum(record => record.TimeSpent.Ticks);

                // Время выполнения всех задач
                var fullRawTimeSpent = list.EstimatedItems.Sum(record => record.TimeSpent.Ticks);
                // Предполагаемый остаток для автоматического списания времени
                var estimatedRestAutoTimeSpent = Convert.ToDecimal(workTime.Ticks - manualTimeSpent);
                //if (fullRawTimeSpent + manualTimeSpent < workTime)
                //{
                //    var percent1 = Convert.ToDecimal(fullRawTimeSpent) / new TimeSpan(9, 0, 0).Ticks;
                //    estimatedRestAutoTimeSpent =
                //        fullRawTimeSpent - new TimeSpan(1, 0, 0).Ticks * percent1 - manualTimeSpent;
                //}

                // Заполняем предполагаемое время для каждой задачи в пропорциях
                foreach (var estimatedWorklog in list.EstimatedItems)
                {
                    if (!estimatedWorklog.ChildItems.Any())
                    {
                        var percent = Convert.ToDecimal(estimatedWorklog.TimeSpent.Ticks) / fullRawTimeSpent;
                        var estimatedTimeSpent = new TimeSpan(Convert.ToInt64(percent * estimatedRestAutoTimeSpent));
                        estimatedWorklog.TimeSpent = estimatedTimeSpent;
                    }
                    else
                    {
                        estimatedWorklog.TimeSpent = new TimeSpan(estimatedWorklog.ChildItems.Sum(item => item.TimeSpent.Ticks));
                    }
                }
            }

            private void Calculate(IEnumerable<DailyWorklogSummary> worklogs, Query query)
            {
                foreach (var worklog in worklogs)
                {
                    var workTime = query.DailyWorkingEndTime - query.DailyWorkingStartTime - TimeSpan.FromHours(1);
                    // Время зафиксированное за день
                    var dayTimeSpent = new TimeSpan(worklog.ActualWorklogs.Sum(record => record.ElapsedTime.Ticks));
                    // Привязка актуальных таймлогов к 
                    foreach (var estimatedWorklog in worklog.EstimatedWorklogs)
                    {
                        estimatedWorklog.ActualWorklogs = worklog.ActualWorklogs
                            .Where(record => record.Issue.Key == estimatedWorklog.Issue.Key
                                             && record.StartedAt == estimatedWorklog.CompletedAt)
                            .ToList();
                    }

                    // Автоматические
                    var autoActualWorklogs = worklog.EstimatedWorklogs.SelectMany(record => record.ActualWorklogs);
                    // Вручную внесенные таймлоги
                    var manualActualWorklogs = worklog.ActualWorklogs.Except(autoActualWorklogs);
                    // Вручную списанное время
                    var manualTimeSpent = manualActualWorklogs.Sum(record => record.ElapsedTime.Ticks);

                    // Время выполнения всех задач
                    var fullRawTimeSpent = worklog.EstimatedWorklogs.Sum(record => record.RawTimeSpent.Ticks);
                    // Предполагаемый остаток для автоматического списания времени
                    var estimatedRestAutoTimeSpent = Convert.ToDecimal(workTime.Ticks - manualTimeSpent);
                    //if (fullRawTimeSpent + manualTimeSpent < workTime)
                    //{
                    //    var percent1 = Convert.ToDecimal(fullRawTimeSpent) / new TimeSpan(9, 0, 0).Ticks;
                    //    estimatedRestAutoTimeSpent =
                    //        fullRawTimeSpent - new TimeSpan(1, 0, 0).Ticks * percent1 - manualTimeSpent;
                    //}

                    // Заполняем предполагаемое время для каждой задачи в пропорциях
                    foreach (var estimatedWorklog in worklog.EstimatedWorklogs)
                    {
                        if (estimatedWorklog.ActualTimeSpent.Ticks == 0)
                        {
                            var percent = Convert.ToDecimal(estimatedWorklog.RawTimeSpent.Ticks) / fullRawTimeSpent;
                            var estimatedTimeSpent = new TimeSpan(Convert.ToInt64(percent * estimatedRestAutoTimeSpent));
                            estimatedWorklog.EstimatedTimeSpent = estimatedTimeSpent;
                        }
                        else
                        {
                            estimatedWorklog.EstimatedTimeSpent = estimatedWorklog.ActualTimeSpent;
                        }
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="rawEstimatedWorklogs"></param>
            /// <param name="StartDate"></param>
            /// <param name="EndDate"></param>
            private IEnumerable<EstimatedWorklog> PrepareEstimatedWorklogs(
                IEnumerable<EstimatedWorklog> rawEstimatedWorklogs,
                Query query)
            {
                List<EstimatedWorklog> estimatedWorklogs = new List<EstimatedWorklog>();
                var startDate = query.EndDate.Date;
                while (startDate >= query.StartDate.Date)
                {
                    //var fromStartDate = startDate.AddHours(10);
                    //var toStartDate = startDate.AddHours(19);
                    var fromStartDate = startDate.Add(query.DailyWorkingStartTime);
                    var toStartDate = startDate.Add(query.DailyWorkingEndTime);
                    var dateWorklogs = rawEstimatedWorklogs
                        .Where(item => item.CompletedAt > fromStartDate
                                       && item.StartedAt < toStartDate)
                        .ToList();
                    foreach (var dateWorklog in dateWorklogs)
                    {
                        var estimatedStartDate = dateWorklog.StartedAt > fromStartDate
                            ? dateWorklog.StartedAt
                            : fromStartDate;
                        var estimatedEndDate = dateWorklog.CompletedAt < toStartDate
                            ? dateWorklog.CompletedAt
                            : toStartDate;
                        estimatedWorklogs.Add(new EstimatedWorklog
                        {
                            StartedAt = estimatedStartDate,
                            CompletedAt = estimatedEndDate,
                            Issue = dateWorklog.Issue
                        });
                    }
                    startDate = startDate.AddDays(-1);
                }

                return estimatedWorklogs;
            }
        }
    }
}
