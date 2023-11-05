using MediatR;
using Pet.Jira.Application.Common.Extensions;
using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Domain.Models.Worklogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Worklogs.Queries
{
    public class GetWorklogCollection
    {
        public class Query : IRequest<Model>
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public TimeSpan DailyWorkingStartTime { get; set; }
            public TimeSpan DailyWorkingEndTime { get; set; }
            public string IssueStatusId { get; set; }
            public TimeSpan CommentWorklogTime { get; set; }
            public TimeSpan LunchTime { get; set; }
        }

        public class Model
        {
            public IEnumerable<WorkingDay> WorkingDays { get; set; }
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
                CancellationToken cancellationToken = default)
            {
                var worklogCollection = await CalculateWorklogCollection(query, cancellationToken);
                return new Model { WorkingDays = worklogCollection };
            }

            private async Task<IEnumerable<WorkingDay>> CalculateWorklogCollection(Query query,
                CancellationToken cancellationToken)
            {
                var rawIssueWorklogs = await _worklogDataSource.GetRawIssueWorklogsAsync(
                    new GetRawIssueWorklogs.Query()
                    {
                        StartDate = query.StartDate,
                        EndDate = query.EndDate,
                        IssueStatusId = query.IssueStatusId,
                        CommentWorklogTime = query.CommentWorklogTime
                    }, cancellationToken);

                var issueWorklogs = await _worklogDataSource.GetIssueWorklogsAsync(
                    new GetIssueWorklogs.Query()
                    {
                        StartDate = query.StartDate,
                        EndDate = query.EndDate
                    }, cancellationToken);

                var days = CalculateDays(issueWorklogs, rawIssueWorklogs, query).ToList();
                foreach (var day in days)
                {
                    day.Refresh();
                }

                return days;
            }

            private static IEnumerable<WorkingDay> CalculateDays(
                IEnumerable<IWorklog> issueWorklogs,
                IEnumerable<IWorklog> rawIssueWorklogs,
                Query query)
            {
                var day = query.EndDate.Date;
                var splitedRawIssueWorklogs = rawIssueWorklogs.SplitByDays(
                    firstDate: query.StartDate,
                    lastDate: query.EndDate);

                while (day >= query.StartDate.Date)
                {
                    var dailyActualWorklogs = issueWorklogs
                        .Where(worklog => worklog.StartDate.Date == day)
                        .Select(worklog => WorkingDayWorklog.CreateActual(worklog));

                    var dailyEstimatedWorklogs = splitedRawIssueWorklogs
                        .Where(worklog => worklog.StartDate.Date == day)
                        .Select(worklog =>
                            WorkingDayWorklog.CreateEstimated(
                                worklog: worklog,
                                day: day,
                                dailyWorkingStartTime: query.DailyWorkingStartTime,
                                dailyWorkingEndTime: query.DailyWorkingEndTime));

                    var dailyWorklogs = dailyActualWorklogs.Union(dailyEstimatedWorklogs)
                            .OrderBy(record => record.StartDate)
                            .ThenBy(record => record.CompleteDate)
                            .ToList();

                    yield return new WorkingDay(
                        date: day,
                        settings: new WorkingDaySettings(
                            workingStartTime: query.DailyWorkingStartTime,
                            workingEndTime: query.DailyWorkingEndTime,
                            lunchTime: query.LunchTime),
                        worklogs: dailyWorklogs);

                    day = day.AddDays(-1);
                }
            }
        }
    }
}