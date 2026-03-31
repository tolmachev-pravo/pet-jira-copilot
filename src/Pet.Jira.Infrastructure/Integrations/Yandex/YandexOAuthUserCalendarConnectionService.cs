using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pet.Jira.Application.Integrations;
using Pet.Jira.Application.Integrations.Dto;
using Pet.Jira.Application.Users;
using Pet.Jira.Application.Users.Dto;
using Pet.Jira.Domain.Entities.Integrations;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Integrations.Yandex
{
	public class YandexOAuthUserCalendarConnectionService : IUserCalendarConnectionService
	{
		private const string StateProtectorPurpose = "Pet.Jira.Yandex.State";
		private const string TokenProtectorPurpose = "Pet.Jira.Yandex.Token";

		private readonly ICurrentAppUserService _currentAppUserService;
		private readonly IUserCalendarConnectionRepository _connectionRepository;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly YandexOAuthConfiguration _configuration;
		private readonly IDataProtector _stateProtector;
		private readonly IDataProtector _tokenProtector;
		private readonly ILogger<YandexOAuthUserCalendarConnectionService> _logger;

		public YandexOAuthUserCalendarConnectionService(
			ICurrentAppUserService currentAppUserService,
			IUserCalendarConnectionRepository connectionRepository,
			IHttpClientFactory httpClientFactory,
			IHttpContextAccessor httpContextAccessor,
			IDataProtectionProvider dataProtectionProvider,
			IOptions<YandexOAuthConfiguration> configuration,
			ILogger<YandexOAuthUserCalendarConnectionService> logger)
		{
			_currentAppUserService = currentAppUserService;
			_connectionRepository = connectionRepository;
			_httpClientFactory = httpClientFactory;
			_httpContextAccessor = httpContextAccessor;
			_configuration = configuration.Value;
			_stateProtector = dataProtectionProvider.CreateProtector(StateProtectorPurpose);
			_tokenProtector = dataProtectionProvider.CreateProtector(TokenProtectorPurpose);
			_logger = logger;
		}

		public async Task<UserCalendarConnectionDto> GetCurrentAsync(CancellationToken cancellationToken = default)
		{
			var user = await GetCurrentAppUserAsync(cancellationToken);
			if (user == null)
			{
				return null;
			}

			var connection = await _connectionRepository.GetByUserIdAsync(user.Id, CalendarProvider.Yandex, cancellationToken);
			if (connection == null)
			{
				return new UserCalendarConnectionDto
				{
					Provider = CalendarProvider.Yandex,
					Status = UserCalendarConnectionStatus.Disconnected
				};
			}

			return new UserCalendarConnectionDto
			{
				Provider = connection.Provider,
				Status = connection.Status,
				ExpiresAtUtc = connection.ExpiresAtUtc,
				Scope = connection.Scope,
				ExternalAccountId = connection.ExternalAccountId,
				ExternalLogin = connection.ExternalLogin,
				LastConnectedAtUtc = connection.LastConnectedAtUtc,
				LastRefreshAtUtc = connection.LastRefreshAtUtc,
				LastError = connection.LastError
			};
		}

		public async Task<string> BuildConnectUrlAsync(CancellationToken cancellationToken = default)
		{
			var user = await GetRequiredAppUserAsync(cancellationToken);
			EnsureConfiguration();

			var state = ProtectState(new YandexOAuthState
			{
				UserId = user.Id,
				CreatedAtUtc = DateTime.UtcNow
			});

			var query = new Dictionary<string, string>
			{
				["response_type"] = "code",
				["client_id"] = _configuration.ClientId,
				["redirect_uri"] = ResolveRedirectUri(),
				["state"] = state
			};

			if (!string.IsNullOrWhiteSpace(_configuration.Scope))
			{
				query["scope"] = _configuration.Scope;
			}

			return QueryHelpers.AddQueryString(YandexAuthenticationDefaults.AuthorizationEndpoint, query);
		}

		public async Task<YandexConnectionResult> HandleCallbackAsync(
			string code,
			string state,
			string error,
			string errorDescription,
			CancellationToken cancellationToken = default)
		{
			var user = await GetRequiredAppUserAsync(cancellationToken);

			try
			{
				EnsureConfiguration();

				if (!string.IsNullOrWhiteSpace(error))
				{
					var callbackError = string.IsNullOrWhiteSpace(errorDescription)
						? error
						: $"{error}: {errorDescription}";
					await MarkErrorAsync(user.Id, callbackError, cancellationToken);
					return YandexConnectionResult.Failure(callbackError);
				}

				var statePayload = UnprotectState(state);
				if (statePayload.UserId != user.Id)
				{
					await MarkErrorAsync(user.Id, "Invalid OAuth state.", cancellationToken);
					return YandexConnectionResult.Failure("Invalid OAuth state.");
				}

				if (string.IsNullOrWhiteSpace(code))
				{
					await MarkErrorAsync(user.Id, "Authorization code was not returned by Yandex.", cancellationToken);
					return YandexConnectionResult.Failure("Authorization code was not returned by Yandex.");
				}

				var tokenResponse = await ExchangeCodeAsync(code, cancellationToken);
				var userInfo = await GetUserInfoAsync(tokenResponse.AccessToken, cancellationToken);

				var connection = await GetOrCreateConnectionAsync(user.Id, cancellationToken);
				connection.Connect(
					accessTokenProtected: _tokenProtector.Protect(tokenResponse.AccessToken),
					refreshTokenProtected: string.IsNullOrWhiteSpace(tokenResponse.RefreshToken)
						? null
						: _tokenProtector.Protect(tokenResponse.RefreshToken),
					expiresAtUtc: tokenResponse.ExpiresIn.HasValue
						? DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn.Value)
						: null,
					scope: string.IsNullOrWhiteSpace(tokenResponse.Scope) ? _configuration.Scope : tokenResponse.Scope,
					externalAccountId: userInfo?.Id,
					externalLogin: userInfo?.GetPreferredLogin());

				await _connectionRepository.UpsertAsync(connection, cancellationToken);
				return YandexConnectionResult.Success();
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "Failed to handle Yandex OAuth callback for user {Username}.", user.Username);
				await MarkErrorAsync(user.Id, "Failed to connect Yandex integration.", cancellationToken);
				return YandexConnectionResult.Failure("Failed to connect Yandex integration.");
			}
		}

		public async Task DisconnectAsync(CancellationToken cancellationToken = default)
		{
			var user = await GetRequiredAppUserAsync(cancellationToken);
			var connection = await GetOrCreateConnectionAsync(user.Id, cancellationToken);
			connection.Disconnect();
			await _connectionRepository.UpsertAsync(connection, cancellationToken);
		}

		private string ResolveRedirectUri()
		{
			if (string.IsNullOrWhiteSpace(_configuration.RedirectUri))
			{
				throw new InvalidOperationException("Yandex OAuth redirect URI is not configured.");
			}

			if (Uri.TryCreate(_configuration.RedirectUri, UriKind.Absolute, out var absoluteUri)
				&& (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps))
			{
				return absoluteUri.ToString();
			}

			if (_configuration.RedirectUri.Contains('?') || _configuration.RedirectUri.Contains('#'))
			{
				throw new InvalidOperationException("Yandex OAuth redirect URI path must not contain query string or fragment.");
			}

			var httpContext = _httpContextAccessor.HttpContext;
			if (httpContext == null)
			{
				throw new InvalidOperationException("Unable to resolve absolute Yandex OAuth redirect URI without an active HTTP request.");
			}

			var path = _configuration.RedirectUri.StartsWith("/")
				? _configuration.RedirectUri
				: $"/{_configuration.RedirectUri}";

			return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.PathBase}{path}";
		}

		private async Task<InfrastructureYandexTokenResponse> ExchangeCodeAsync(string code, CancellationToken cancellationToken)
		{
			using var request = new HttpRequestMessage(HttpMethod.Post, YandexAuthenticationDefaults.TokenEndpoint)
			{
				Content = new FormUrlEncodedContent(new Dictionary<string, string>
				{
					["grant_type"] = "authorization_code",
					["code"] = code,
					["client_id"] = _configuration.ClientId,
					["client_secret"] = _configuration.ClientSecret
				})
			};

			using var client = _httpClientFactory.CreateClient();
			using var response = await client.SendAsync(request, cancellationToken);
			var payload = await response.Content.ReadAsStringAsync();
			if (!response.IsSuccessStatusCode)
			{
				throw new InvalidOperationException($"Yandex token exchange failed: {(int)response.StatusCode}.");
			}

			var tokenResponse = JsonSerializer.Deserialize<InfrastructureYandexTokenResponse>(payload, JsonOptions());
			if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
			{
				throw new InvalidOperationException("Yandex token exchange returned an invalid payload.");
			}

			return tokenResponse;
		}

		private async Task<InfrastructureYandexUserInfoResponse> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken)
		{
			using var request = new HttpRequestMessage(
				HttpMethod.Get,
				QueryHelpers.AddQueryString(
					YandexAuthenticationDefaults.UserInformationEndpoint,
					new Dictionary<string, string> { ["format"] = "json" }));

			request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", accessToken);

			using var client = _httpClientFactory.CreateClient();
			using var response = await client.SendAsync(request, cancellationToken);
			var payload = await response.Content.ReadAsStringAsync();
			if (!response.IsSuccessStatusCode)
			{
				throw new InvalidOperationException($"Yandex user info request failed: {(int)response.StatusCode}.");
			}

			return JsonSerializer.Deserialize<InfrastructureYandexUserInfoResponse>(payload, JsonOptions());
		}

		private Task<UserDto> GetCurrentAppUserAsync(CancellationToken cancellationToken)
		{
			return _currentAppUserService.GetOrCreateCurrentAsync(cancellationToken);
		}

		private async Task<UserDto> GetRequiredAppUserAsync(CancellationToken cancellationToken)
		{
			var user = await GetCurrentAppUserAsync(cancellationToken);
			if (user == null)
			{
				throw new InvalidOperationException("Current application user was not found.");
			}

			return user;
		}

		private async Task<UserCalendarConnection> GetOrCreateConnectionAsync(Guid userId, CancellationToken cancellationToken)
		{
			var connection = await _connectionRepository.GetByUserIdAsync(userId, CalendarProvider.Yandex, cancellationToken);
			if (connection != null)
			{
				return connection;
			}

			return new UserCalendarConnection
			{
				UserId = userId,
				Provider = CalendarProvider.Yandex,
				Status = UserCalendarConnectionStatus.Disconnected,
				CreatedAt = DateTime.UtcNow
			};
		}

		private async Task MarkErrorAsync(Guid userId, string error, CancellationToken cancellationToken)
		{
			var connection = await GetOrCreateConnectionAsync(userId, cancellationToken);
			connection.MarkError(error);
			await _connectionRepository.UpsertAsync(connection, cancellationToken);
		}

		private string ProtectState(YandexOAuthState state)
		{
			var json = JsonSerializer.Serialize(state);
			var protectedState = _stateProtector.Protect(json);
			return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(protectedState));
		}

		private YandexOAuthState UnprotectState(string state)
		{
			if (string.IsNullOrWhiteSpace(state))
			{
				throw new InvalidOperationException("OAuth state is missing.");
			}

			var protectedState = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(state));
			var json = _stateProtector.Unprotect(protectedState);
			var payload = JsonSerializer.Deserialize<YandexOAuthState>(json, JsonOptions());
			if (payload == null)
			{
				throw new InvalidOperationException("OAuth state is invalid.");
			}

			return payload;
		}

		private void EnsureConfiguration()
		{
			if (string.IsNullOrWhiteSpace(_configuration.ClientId)
				|| string.IsNullOrWhiteSpace(_configuration.ClientSecret)
				|| string.IsNullOrWhiteSpace(_configuration.RedirectUri))
			{
				throw new InvalidOperationException("Yandex OAuth is not configured.");
			}
		}

		private static JsonSerializerOptions JsonOptions()
		{
			return new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			};
		}

		private class YandexOAuthState
		{
			public Guid UserId { get; set; }
			public DateTime CreatedAtUtc { get; set; }
		}

		private class InfrastructureYandexTokenResponse
		{
			[JsonPropertyName("access_token")]
			public string AccessToken { get; set; }

			[JsonPropertyName("refresh_token")]
			public string RefreshToken { get; set; }

			[JsonPropertyName("expires_in")]
			public int? ExpiresIn { get; set; }

			[JsonPropertyName("scope")]
			public string Scope { get; set; }
		}

		private class InfrastructureYandexUserInfoResponse
		{
			[JsonPropertyName("id")]
			public string Id { get; set; }

			[JsonPropertyName("login")]
			public string Login { get; set; }

			[JsonPropertyName("default_email")]
			public string DefaultEmail { get; set; }

			[JsonPropertyName("emails")]
			public string[] Emails { get; set; }

			public string GetPreferredLogin()
			{
				if (!string.IsNullOrWhiteSpace(DefaultEmail))
				{
					return DefaultEmail;
				}

				if (Emails?.Length > 0 && !string.IsNullOrWhiteSpace(Emails[0]))
				{
					return Emails[0];
				}

				return Login;
			}
		}
	}
}
