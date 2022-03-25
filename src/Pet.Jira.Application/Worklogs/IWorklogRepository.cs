using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Domain.Models.Worklogs;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Worklogs
{
    public interface IWorklogRepository
    {
        Task AddAsync(AddedWorklogDto worklog);
    }
}
