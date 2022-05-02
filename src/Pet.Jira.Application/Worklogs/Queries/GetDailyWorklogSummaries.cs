using MediatR;
using Pet.Jira.Application.Extensions;
using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Domain.Models.Worklogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

                var days = CalculateDays(issueWorklogs, rawIssueWorklogs, query).ToList();
                foreach (var day in days)
                {
                    day.Refresh();
                }

                return new WorklogCollection() { Days = days.ToList() };
            }

            private IEnumerable<WorklogCollectionDay> CalculateDays(
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
                        .Where(worklog => worklog.StartDate.Date == day)
                        .Select(worklog => WorklogCollectionItem.Create(worklog, WorklogCollectionItemType.Actual));

                    var dailyRawIssueWorklogs = splitedRawIssueWorklogs
                        .Where(worklog => worklog.StartDate.Date == day)
                        .Select(worklog =>
                            WorklogCollectionItem.Create(worklog, WorklogCollectionItemType.Estimated,
                                dailyIssueWorklogs));

                    yield return new WorklogCollectionDay
                    {
                        Date = day,
                        Items = dailyIssueWorklogs.Union(dailyRawIssueWorklogs).ToList(),
                        DailyWorkingStartTime = query.DailyWorkingStartTime,
                        DailyWorkingEndTime = query.DailyWorkingEndTime
                    };
                    day = day.AddDays(-1);
                }
            }
        }
    }
}