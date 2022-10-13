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
        public Task AddAsync(AddedWorklogDto worklog, CancellationToken cancellationToken = default)
        {
            MockWorklogStorage.IssueWorklogs.Add(new IssueWorklog
            {
                StartDate = worklog.StartedAt,
                Issue = new Issue(){Key = worklog.IssueKey},
                TimeSpent = worklog.ElapsedTime,
                CompleteDate = worklog.StartedAt.Add(worklog.ElapsedTime)
            });
            return Task.CompletedTask;
        }
    }
}
