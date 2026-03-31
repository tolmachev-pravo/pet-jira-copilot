using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Users;
using Pet.Jira.Application.Users.Dto;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Users
{
	public class CurrentAppUserService : ICurrentAppUserService
	{
		private readonly IIdentityService _identityService;
		private readonly IUserRepository _userRepository;

		public CurrentAppUserService(
			IIdentityService identityService,
			IUserRepository userRepository)
		{
			_identityService = identityService;
			_userRepository = userRepository;
		}

		public async Task<UserDto> GetOrCreateCurrentAsync(CancellationToken cancellationToken = default)
		{
			var identityUser = await _identityService.GetCurrentUserAsync();
			if (identityUser == null)
			{
				return null;
			}

			if (string.IsNullOrWhiteSpace(identityUser.Username))
			{
				throw new InvalidOperationException("Current authenticated user does not have a username.");
			}

			var user = await _userRepository.GetOrCreateByUsernameAsync(identityUser.Username, cancellationToken);
			return new UserDto
			{
				Id = user.Id,
				Username = user.Username
			};
		}
	}
}
