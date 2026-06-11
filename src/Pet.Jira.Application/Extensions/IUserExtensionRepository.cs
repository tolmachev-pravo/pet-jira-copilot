using Pet.Jira.Domain.Entities.Extensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Extensions
{
    public interface IUserExtensionRepository
    {
        Task<UserExtension?> GetAsync(string username, ExtensionType type, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<UserExtension>> GetAllAsync(string username, CancellationToken cancellationToken = default);
        Task UpsertAsync(UserExtension extension, CancellationToken cancellationToken = default);
    }
}
