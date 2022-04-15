using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Authentication.Dto;
using System;
using System.Collections.Concurrent;

namespace Pet.Jira.Infrastructure.Authentication
{
    public class LoginStorage : ILoginStorage
    {
        private readonly ConcurrentDictionary<Guid, LoginDto> _storage = new ConcurrentDictionary<Guid, LoginDto>();

        public bool TryAdd(LoginDto dto) => _storage.TryAdd(dto.Id, dto);

        public bool TryGetValue(Guid id, out LoginDto dto) => _storage.TryGetValue(id, out dto);

        public bool TryRemove(Guid id, out LoginDto dto) => _storage.TryRemove(id, out dto);
    }
}
