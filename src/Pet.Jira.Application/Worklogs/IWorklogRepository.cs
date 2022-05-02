using Pet.Jira.Application.Worklogs.Dto;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Worklogs
{
    public interface IWorklogRepository
    {
        Task AddAsync(AddedWorklogDto worklog);
    }
}
