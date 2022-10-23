using Pet.Jira.Application.Storage;
using Pet.Jira.Application.Worklogs.Dto;

namespace Pet.Jira.Application.Worklogs
{
    public class UserWorklogFilterMemoryCache : BaseMemoryCache<string, UserWorklogFilter>
    {
    }
}
