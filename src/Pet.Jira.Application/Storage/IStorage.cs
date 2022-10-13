namespace Pet.Jira.Application.Storage
{
    public interface IStorage<TKey, TEntity>
    {
        bool TryAdd(TEntity entity);
        bool TryGetValue(TKey key, out TEntity entity);
        bool TryRemove(TKey key, out TEntity entity);
    }
}
