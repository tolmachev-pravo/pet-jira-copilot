using System.Threading.Tasks;
using System.Threading;

namespace Pet.Jira.Application.Storage
{
    public interface IStorage<TKey, TEntity>
    {
        Task<bool> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task<TEntity> GetValueAsync(TKey key, CancellationToken cancellationToken = default);
        Task<bool> RemoveAsync(TKey key, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(TKey key, TEntity newEntity, CancellationToken cancellationToken = default);
    }
}
