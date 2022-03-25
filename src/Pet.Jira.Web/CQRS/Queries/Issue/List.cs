using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Pet.Jira.Adapter;

namespace Pet.Jira.Web.CQRS.Queries.Issue
{
    public class List
    {
        public class Query : IRequest<Model>
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int Count { get; set; }
        }

        public class Model
        {
            public IEnumerable<DayUserWorklog> Worklogs { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, Model>
        {
            private readonly JiraService _jiraService;

            public QueryHandler(JiraService jiraService)
            {
                _jiraService = jiraService;
            }

            public async Task<Model> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                var worklogs = await _jiraService.GetUserDayWorklogs(request.StartDate, request.EndDate, request.Count);
                return new Model { Worklogs = worklogs };
            }
        }
    }
}
