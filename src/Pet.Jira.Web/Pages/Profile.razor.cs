using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using MudBlazor;
using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Integrations;
using Pet.Jira.Application.Storage;
using Pet.Jira.Domain.Entities.Integrations;
using Pet.Jira.Domain.Models.Users;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Pages
{
	public partial class Profile
	{
		private readonly ComponentModel _model = new();

		[Inject] private IIdentityService IdentityService { get; set; }
		[Inject] private IStorage<string, UserProfile> UserProfileStorage { get; set; }
		[Inject] private IUserCalendarConnectionService UserCalendarConnectionService { get; set; }
		[Inject] private NavigationManager NavigationManager { get; set; }

		protected override async Task OnInitializedAsync()
		{
			var currentUser = await IdentityService.GetCurrentUserAsync();
			if (currentUser != null)
			{
				_model.Username = currentUser.Username;
				var profile = await UserProfileStorage.GetValueAsync(currentUser.Key);
				if (profile != null)
				{
					_model.Avatar = profile.Avatar;
					_model.Username = string.IsNullOrWhiteSpace(profile.Username) ? _model.Username : profile.Username;
				}
			}

			var connection = await UserCalendarConnectionService.GetCurrentAsync();
			_model.Apply(connection);
			_model.ApplyStatusMessage(ReadStatusFromQuery());
		}

		private string ReadStatusFromQuery()
		{
			var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
			var query = QueryHelpers.ParseQuery(uri.Query);
			return query.TryGetValue("yandexStatus", out var status)
				? status.FirstOrDefault()
				: null;
		}

		private class ComponentModel
		{
			public string Username { get; set; }
			public string Avatar { get; set; }
			public string ExternalLogin { get; set; }
			public DateTime? LastConnectedAtUtc { get; set; }
			public string LastError { get; set; }
			public UserCalendarConnectionStatus ConnectionStatus { get; set; } = UserCalendarConnectionStatus.Disconnected;
			public string StatusMessage { get; set; }
			public Severity StatusSeverity { get; set; } = Severity.Info;

			public string ConnectionStatusText => ConnectionStatus switch
			{
				UserCalendarConnectionStatus.Connected => "Подключено",
				UserCalendarConnectionStatus.Error => "Ошибка подключения",
				_ => "Не подключено"
			};

			public bool CanConnect => ConnectionStatus == UserCalendarConnectionStatus.Disconnected;
			public bool CanReconnect => ConnectionStatus == UserCalendarConnectionStatus.Connected || ConnectionStatus == UserCalendarConnectionStatus.Error;
			public bool CanDisconnect => ConnectionStatus != UserCalendarConnectionStatus.Disconnected;

			public void Apply(Pet.Jira.Application.Integrations.Dto.UserCalendarConnectionDto connection)
			{
				if (connection == null)
				{
					return;
				}

				ConnectionStatus = connection.Status;
				ExternalLogin = connection.ExternalLogin;
				LastConnectedAtUtc = connection.LastConnectedAtUtc;
				LastError = connection.LastError;
			}

			public void ApplyStatusMessage(string status)
			{
				switch (status)
				{
					case "connected":
						StatusMessage = "Интеграция Yandex успешно подключена.";
						StatusSeverity = Severity.Success;
						break;
					case "disconnected":
						StatusMessage = "Интеграция Yandex отключена.";
						StatusSeverity = Severity.Info;
						break;
					case "error":
						StatusMessage = "Не удалось подключить Yandex. Проверьте данные приложения и повторите попытку.";
						StatusSeverity = Severity.Error;
						break;
				}
			}
		}
	}
}
