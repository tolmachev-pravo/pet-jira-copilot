using Pet.Jira.Application.Storage;
using Pet.Jira.Domain.Models.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Storage
{
    public abstract class BaseStorage<TKey, TEntity> : IStorage<TKey, TEntity>
        where TEntity : IEntity<TKey>
    {
        private readonly ILocalStorage<TEntity> _localStorage;
        private readonly IMemoryCache<TKey, TEntity> _memoryCache;

        public BaseStorage(
            ILocalStorage<TEntity> localStorage,
            IMemoryCache<TKey, TEntity> memoryCache)
        {
            _localStorage = localStorage;
            _memoryCache = memoryCache;
        }

        public virtual async Task<bool> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (!Equals(entity.Key, default(TKey)))
            {
                return await _localStorage.AddAsync(entity, cancellationToken)
                    && _memoryCache.TryAdd(entity);
            }
            else
            {
                return await _localStorage.AddAsync(entity, cancellationToken);
            }
        }

        public virtual async Task<TEntity> GetValueAsync(TKey key, CancellationToken cancellationToken = default) 
        {
            if (!Equals(key, default(TKey)))
            {
                if (_memoryCache.TryGetValue(key, out TEntity entity))
                {
                    return entity;
                }

                entity = await _localStorage.GetValueAsync(cancellationToken);
                if (entity != null)
                {
                    _memoryCache.TryAdd(entity);
                }
                return entity;
            }
            else
            {
                return await _localStorage.GetValueAsync(cancellationToken);
            }
        }

        public virtual async Task<bool> RemoveAsync(TKey key, CancellationToken cancellationToken = default) 
        {
            if (!Equals(key, default(TKey)))
            {                
                return await _localStorage.RemoveAsync(cancellationToken)
                    && _memoryCache.TryRemove(key, out _);
            }
            else
            {
                return await _localStorage.RemoveAsync(cancellationToken);
            }
        }

        public virtual async Task<bool> UpdateAsync(TKey key, TEntity newEntity, CancellationToken cancellationToken = default)
        {
            if (!Equals(key, default(TKey)))
            {                
                return await _localStorage.UpdateAsync(newEntity, cancellationToken) 
                    && _memoryCache.TryUpdate(key, newEntity);
            }
            else
            {
                return await _localStorage.UpdateAsync(newEntity, cancellationToken);
            }
        }
    }
}
