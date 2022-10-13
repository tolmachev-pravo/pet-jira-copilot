using Pet.Jira.Application.Time;
using Pet.Jira.Application.Users;
using Pet.Jira.Application.Worklogs;
using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Infrastructure.Jira;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Worklogs
{
    public class WorklogRepository : IWorklogRepository
    {
        private readonly IJiraService _jiraService;
        private readonly ITimeProvider _timeProvider;
        private readonly IUserDataSource _userDataSource;

        public WorklogRepository(
            IJiraService jiraService,
            ITimeProvider timeProvider,
            IUserDataSource userDataSource)
        {
            _jiraService = jiraService;
            _timeProvider = timeProvider;
            _userDataSource = userDataSource;
        }

        public async Task AddAsync(AddedWorklogDto worklog, CancellationToken cancellationToken = default)
        {
            var user = await _userDataSource.GetCurrentUserAsync(cancellationToken);
            worklog.StartedAt = _timeProvider.ConvertToServerTimezone(worklog.StartedAt.ToLocalTime(), user.TimeZoneInfo);
            await _jiraService.AddWorklogAsync(worklog);
        }
    }
}
