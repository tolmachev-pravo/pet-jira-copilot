using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Web.Components.Clipboard;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Worklogs
{
    public partial class WorklogMenu : ComponentBase
    {
        [Parameter] public WorklogCollectionItem Entity { get; set; }
        [Parameter] public Color Color { get; set; } = Color.Default;
        [Inject] private IClipboard _clipboard { get; set; }
        [Inject] private ISnackbar _snackbar { get; set; }

        private async Task CopyToClipboardInReviewAsync(WorklogCollectionItem entity)
        {
            var text = $"**IN REVIEW** [{entity.Issue.Key}]({entity.Issue.Link}) {entity.Issue.Summary}";
            await CopyToClipboardAsync(text);
        }

        private async Task CopyToClipboardInProgressAsync(WorklogCollectionItem entity)
        {
            var text = $"**IN PROGRESS** [{entity.Issue.Key}]({entity.Issue.Link}) {entity.Issue.Summary}";
            await CopyToClipboardAsync(text);
        }

        private async Task CopyToClipboardAsync(string text)
        {
            await _clipboard.CopyToAsync(text);
            _snackbar.Add(
                $"Copied to clipboard",
                Severity.Success,
                config => { config.ActionColor = Color.Success; });
        }
    }
}
