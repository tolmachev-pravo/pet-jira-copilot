using System;
using MediatR;
using Pet.Jira.Domain.Models.Issues;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Issues.Queries
{
    public class GetIssueStatuses
    {
        public class Query : IRequest<Model>
        {
        }

        public class Model
        {
            public IEnumerable<IssueStatus> IssueStatuses { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, Model>
        {
            private readonly IIssueDataSource _issueDataSource;

            public QueryHandler(IIssueDataSource issueDataSource)
            {
                _issueDataSource = issueDataSource;
            }

            public async Task<Model> Handle(
                Query query,
                CancellationToken cancellationToken)
            {
                try
                {
                    var issueStatuses = await _issueDataSource.GetIssueStatusesAsync(query, cancellationToken);
                    return new Model { IssueStatuses = issueStatuses.OrderBy(record => record.Name) };
                }
                catch (AuthenticationException e)
                {
                    throw new Exception($"Authentication exception") { Source = e.Source };
                }
            }
        }
    }
}
