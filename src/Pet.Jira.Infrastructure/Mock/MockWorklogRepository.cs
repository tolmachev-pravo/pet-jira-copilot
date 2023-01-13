using Microsoft.Extensions.Logging;
using Pet.Jira.Application.Worklogs;
using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Domain.Models.Issues;
using Pet.Jira.Domain.Models.Worklogs;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Mock
{
    internal class MockWorklogRepository : IWorklogRepository
    {
        private readonly ILogger<MockWorklogRepository> _logger;

        public MockWorklogRepository(ILogger<MockWorklogRepository> logger)
        {
            _logger = logger;
        }

        public Task AddAsync(AddedWorklogDto worklog, CancellationToken cancellationToken = default)
        {
            MockWorklogStorage.IssueWorklogs.Add(new IssueWorklog
            {
                StartDate = worklog.StartedAt,
                Issue = new Issue(){Key = worklog.IssueKey},
                TimeSpent = worklog.ElapsedTime,
                CompleteDate = worklog.StartedAt.Add(worklog.ElapsedTime)
            });
            _logger.LogInformation("Worklog added successfully. {@entity}", worklog);
            return Task.CompletedTask;
        }
    }
}
