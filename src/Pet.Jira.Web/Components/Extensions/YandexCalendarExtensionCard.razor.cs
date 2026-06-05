using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Extensions.Commands;
using Pet.Jira.Application.Extensions.Dto;
using Pet.Jira.Application.Extensions.Queries;
using Pet.Jira.Domain.Entities.Extensions;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Extensions
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
            var settings = await Mediator.Send(
                new GetExtension.Query(_username, ExtensionType.YandexCalendar));
            _isEnabled = settings is not null;
            await NotifyStateChangedAsync();
        }

        private async Task NotifyStateChangedAsync()
        {
            if (StateChanged.HasDelegate)
                await StateChanged.InvokeAsync(_isEnabled);
        }

        private async Task OnToggleChanged(bool value)
        {
            var settings = await Mediator.Send(
                new GetExtension.Query(_username, ExtensionType.YandexCalendar));

            if (settings is null && value)
            {
                Snackbar.Add("Сначала настройте расширение", Severity.Info);
                _isEnabled = false;
                return;
            }

            if (settings is not null)
            {
                await Mediator.Send(new UpsertExtension.Command(_username, settings, value));
                _isEnabled = value;
                await NotifyStateChangedAsync();
            }
        }

        private async Task OpenSettingsDialog()
        {
            var settings = await Mediator.Send(
                new GetExtension.Query(_username, ExtensionType.YandexCalendar));

            var parameters = new DialogParameters
            {
                { nameof(YandexCalendarSettingsDialog.Username), _username },
                { nameof(YandexCalendarSettingsDialog.ExistingSettings), settings }
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
