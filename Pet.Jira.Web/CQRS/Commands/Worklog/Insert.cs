using MediatR;
using Pet.Jira.Adapter;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Web.CQRS.Commands.Worklog
{
    public class Insert
    {
        public class Command : IRequest<Model>
        {
            public Command(Model model)
            {
                _model = model;
            }

            private readonly Model _model;
            public Model Model => _model;
        }

        public class Model
        {
            public string IssueKey { get; set; }
            public TimeSpan TimeSpent { get; set; }
            public DateTime At { get; set; }
        }

        public class Handler : IRequestHandler<Command, Model>
        {
            private readonly JiraService _jiraService;

            public Handler(JiraService jiraService)
            {
                _jiraService = jiraService;
            }

            public async Task<Model> Handle(
                Command request,
                CancellationToken cancellationToken)
            {
                await _jiraService.AddTimeLog(request.Model.IssueKey, request.Model.TimeSpent, request.Model.At);
                return request.Model;
            }
        }
    }
}
