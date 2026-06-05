using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pet.Jira.Application.Extensions.YandexCalendar.Commands;
using Pet.Jira.Application.Extensions.YandexCalendar.Dto;
using Pet.Jira.Application.Extensions.YandexCalendar.Queries;
using System;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Extensions.YandexCalendar
{
    public partial class YandexCalendarSettingsDialog : ComponentBase
    {
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; } = default!;

        [Parameter] public string Username { get; set; } = string.Empty;
        [Parameter] public YandexCalendarSettingsDto? ExistingSettings { get; set; }

        [Inject] private IMediator Mediator { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;

        private MudForm _form = default!;
        private string _login = string.Empty;
        private string _appPassword = string.Empty;

        protected override void OnParametersSet()
        {
            if (ExistingSettings is not null)
            {
                _login = ExistingSettings.Login;
                _appPassword = ExistingSettings.AppPassword;
            }
        }

        private async Task TestConnection()
        {
            if (string.IsNullOrWhiteSpace(_login) || string.IsNullOrWhiteSpace(_appPassword))
            {
                Snackbar.Add("Заполните логин и пароль", Severity.Warning);
                return;
            }
            try
            {
                var events = await Mediator.Send(new GetYandexCalendarEvents.Query(
                    Username,
                    DateOnly.FromDateTime(DateTime.Today)));
                Snackbar.Add($"Подключено! Найдено {events.Count} событий на сегодня", Severity.Success);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Ошибка подключения: {ex.Message}", Severity.Error);
            }
        }

        private async Task Save()
        {
            await _form.Validate();
            if (!_form.IsValid) return;

            var settings = new YandexCalendarSettingsDto(_login, _appPassword);
            await Mediator.Send(new UpsertYandexCalendarExtension.Command(Username, settings, IsEnabled: true));
            MudDialog.Close(DialogResult.Ok(settings));
        }

        private void Cancel() => MudDialog.Cancel();
    }
}
