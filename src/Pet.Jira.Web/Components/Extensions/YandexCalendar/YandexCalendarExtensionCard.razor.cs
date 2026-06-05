using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Extensions.YandexCalendar.Commands;
using Pet.Jira.Application.Extensions.YandexCalendar.Dto;
using Pet.Jira.Application.Extensions.YandexCalendar.Queries;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Extensions.YandexCalendar
{
    public partial class YandexCalendarExtensionCard : ComponentBase
    {
        [Inject] private IMediator Mediator { get; set; } = default!;
        [Inject] private IDialogService DialogService { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;
        [Inject] private IIdentityService IdentityService { get; set; } = default!;

        [Parameter] public EventCallback<bool> StateChanged { get; set; }

        private bool _isEnabled;
        private string _username = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            _username = IdentityService.CurrentUser?.Username ?? string.Empty;
            var extension = await Mediator.Send(new GetYandexCalendarSettings.Query(_username));
            _isEnabled = extension.IsEnabled;
            await NotifyStateChangedAsync();
        }

        private async Task NotifyStateChangedAsync()
        {
            if (StateChanged.HasDelegate)
                await StateChanged.InvokeAsync(_isEnabled);
        }

        private async Task OnToggleChanged(bool value)
        {
            var extension = await Mediator.Send(new GetYandexCalendarSettings.Query(_username));

            if (extension.Settings is null && value)
            {
                Snackbar.Add("Сначала настройте расширение", Severity.Info);
                _isEnabled = false;
                return;
            }

            if (extension.Settings is not null)
            {
                await Mediator.Send(new UpsertYandexCalendarExtension.Command(_username, extension.Settings, value));
                _isEnabled = value;
                await NotifyStateChangedAsync();
            }
        }

        private async Task OpenSettingsDialog()
        {
            var extension = await Mediator.Send(new GetYandexCalendarSettings.Query(_username));

            var parameters = new DialogParameters
            {
                { nameof(YandexCalendarSettingsDialog.Username), _username },
                { nameof(YandexCalendarSettingsDialog.ExistingSettings), extension.Settings }
            };
            var dialog = await DialogService.ShowAsync<YandexCalendarSettingsDialog>(
                "Яндекс Календарь — настройки", parameters);
            var result = await dialog.Result;
            if (!result.Cancelled)
            {
                _isEnabled = result.Data is YandexCalendarSettingsDto;
                await NotifyStateChangedAsync();
                StateHasChanged();
            }
        }
    }
}
