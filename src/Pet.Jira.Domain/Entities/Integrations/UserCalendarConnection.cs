using Pet.Jira.Domain.Entities.Users;
using System;

namespace Pet.Jira.Domain.Entities.Integrations
{
	public class UserCalendarConnection : BaseEntity
	{
		public Guid UserId { get; set; }
		public User User { get; set; }

		public CalendarProvider Provider { get; set; }
		public UserCalendarConnectionStatus Status { get; set; }

		public string AccessTokenProtected { get; set; }
		public string RefreshTokenProtected { get; set; }
		public DateTime? ExpiresAtUtc { get; set; }
		public string Scope { get; set; }

		public string ExternalAccountId { get; set; }
		public string ExternalLogin { get; set; }

		public DateTime? LastConnectedAtUtc { get; set; }
		public DateTime? LastRefreshAtUtc { get; set; }
		public string LastError { get; set; }

		public void Connect(
			string accessTokenProtected,
			string refreshTokenProtected,
			DateTime? expiresAtUtc,
			string scope,
			string externalAccountId,
			string externalLogin)
		{
			AccessTokenProtected = accessTokenProtected;
			RefreshTokenProtected = refreshTokenProtected;
			ExpiresAtUtc = expiresAtUtc;
			Scope = scope;
			ExternalAccountId = externalAccountId;
			ExternalLogin = externalLogin;
			Status = UserCalendarConnectionStatus.Connected;
			LastConnectedAtUtc = DateTime.UtcNow;
			LastRefreshAtUtc = DateTime.UtcNow;
			LastError = null;
			UpdatedAt = DateTime.UtcNow;
		}

		public void MarkError(string error)
		{
			Status = UserCalendarConnectionStatus.Error;
			LastError = error;
			UpdatedAt = DateTime.UtcNow;
		}

		public void Disconnect()
		{
			Status = UserCalendarConnectionStatus.Disconnected;
			AccessTokenProtected = null;
			RefreshTokenProtected = null;
			ExpiresAtUtc = null;
			Scope = null;
			ExternalAccountId = null;
			ExternalLogin = null;
			LastError = null;
			UpdatedAt = DateTime.UtcNow;
		}
	}
}
