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
            public string Jql { get; set; }
            public int Count { get; set; }
        }

        public class Model
        {
            public IEnumerable<TimeLog> TimeLogs { get; set; }
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
                var test = await _jiraService.GetCalculatedIssueTimeLogs(request.Jql, request.Count);
                var timeLogs = test.SelectMany(item => item.Value);
                return new Model { TimeLogs = timeLogs };
            }
        }
    }
}
