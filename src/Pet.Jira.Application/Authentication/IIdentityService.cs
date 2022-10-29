using Pet.Jira.Domain.Models.Users;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Authentication
{
    public interface IIdentityService
    {
        Task<User> GetCurrentUserAsync();
        User CurrentUser { get; }
    }
}
