using Pet.Jira.Application.Users;
using Pet.Jira.Domain.Models.Users;
using System.Collections.Concurrent;

namespace Pet.Jira.Infrastructure.Jira
{
    internal class JiraUserStorage : IUserStorage
    {
        private readonly ConcurrentDictionary<string, User> _storage = new();

        public bool TryAdd(User entity) => _storage.TryAdd(entity.Username, entity);

        public bool TryGetValue(string key, out User entity) => _storage.TryGetValue(key, out entity);

        public bool TryRemove(string key, out User entity) => _storage.TryRemove(key, out entity);
    }
}
