using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Collections.Generic;

namespace Pet.Jira.Web.Components.Extensions
{
    public partial class ExtensionsPage : ComponentBase
    {
        private int _activeCount;

        private int TotalSlots => _comingSoon.Count + 1; // +1 = Yandex Calendar

        private readonly List<ComingSoonSlot> _comingSoon = new()
        {
            new("Jira Smart Sync", "Двусторонняя синхронизация ворклогов с задачами Jira в реальном времени.", Icons.Material.Filled.Sync),
            new("GitLab Activity", "Подтягивайте коммиты и merge-request'ы как кандидатов для списания времени.", Icons.Material.Filled.MergeType),
            new("Outlook Calendar", "Импорт встреч из Microsoft 365 через Graph API.", Icons.Material.Filled.Event),
        };

        private void OnYandexStateChanged(bool enabled)
        {
            _activeCount = enabled ? 1 : 0;
            StateHasChanged();
        }

        private sealed record ComingSoonSlot(string Title, string Description, string Icon);
    }
}
