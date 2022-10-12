using Pet.Jira.Application.Authentication.Dto;
using Pet.Jira.Application.Storage;
using System;

namespace Pet.Jira.Application.Authentication
{
    public interface ILoginStorage : IStorage<Guid, LoginDto>
    {
    }
}
