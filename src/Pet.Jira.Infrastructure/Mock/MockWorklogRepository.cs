using Pet.Jira.Application.Worklogs;
using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Domain.Models.Issues;
using Pet.Jira.Domain.Models.Worklogs;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Mock
{
    internal class MockWorklogRepository : IWorklogRepository
    {
        public async Task AddAsync(AddedWorklogDto worklog)
        {
            MockWorklogStorage.IssueWorklogs.Add(new IssueWorklog
            {
                StartedAt = worklog.StartedAt,
                Issue = new Issue(){Key = worklog.IssueKey},
                ElapsedTime = worklog.ElapsedTime,
                CompletedAt = worklog.StartedAt.Add(worklog.ElapsedTime)
            });
        }
    }
}
