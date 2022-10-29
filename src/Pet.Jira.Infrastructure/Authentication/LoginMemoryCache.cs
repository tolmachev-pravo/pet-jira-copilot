using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Authentication.Dto;
using Pet.Jira.Application.Storage;
using System;

namespace Pet.Jira.Infrastructure.Authentication
{
    public class LoginMemoryCache : BaseMemoryCache<Guid, LoginDto>, ILoginMemoryCache
    {
    }
}
