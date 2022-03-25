using MediatR;
using Pet.Jira.Domain.Models.Worklogs;
using System;
using System.Collections.Generic;
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
            public int Count { get; set; }
        }

        public class Model
        {
            public IEnumerable<DailyWorklogSummary> Worklogs { get; set; }
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
                var worklogs = await _worklogDataSource.GetUserDayWorklogs(query.StartDate, query.EndDate, query.Count);
                return new Model { Worklogs = worklogs };
            }
        }
    }
}
