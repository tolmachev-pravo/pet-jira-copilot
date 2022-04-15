using Pet.Jira.Application.Authentication.Dto;
using System;

namespace Pet.Jira.Application.Authentication
{
    public interface ILoginStorage
    {
        bool TryAdd(LoginDto dto);
        bool TryGetValue(Guid id, out LoginDto dto);
        bool TryRemove(Guid id, out LoginDto dto);
    }
}
