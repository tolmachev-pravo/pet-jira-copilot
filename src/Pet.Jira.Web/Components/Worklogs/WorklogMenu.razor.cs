using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Web.Components.Clipboard;
using Pet.Jira.Web.Shared;
using System;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Worklogs
{
    public partial class WorklogMenu : ComponentBase
    {
        [Parameter] public WorklogCollectionItem Entity { get; set; }
        [Parameter] public Color Color { get; set; } = Color.Default;
        [Parameter] public string Icon { get; set; } = Icons.Outlined.ArrowCircleDown;

        [CascadingParameter] public ErrorHandler ErrorHandler { get; set; }

        [Inject] private IClipboard _clipboard { get; set; }
        [Inject] private ISnackbar _snackbar { get; set; }

        private async Task CopyToClipboardInReviewAsync()
        {
            var text = $"**IN REVIEW** [{Entity.Issue.Key}]({Entity.Issue.Link}) {Entity.Issue.Summary}";
            await CopyToClipboardAsync(text);
        }

        private async Task CopyToClipboardInProgressAsync()
        {
            var text = $"**IN PROGRESS** [{Entity.Issue.Key}]({Entity.Issue.Link}) {Entity.Issue.Summary}";
            await CopyToClipboardAsync(text);
        }

        private async Task CopyToClipboardAsync(string text)
        {
            try
            {
                await _clipboard.CopyToAsync(text);
                _snackbar.Add(
                    $"Copied to clipboard",
                    Severity.Success,
                    config => { config.ActionColor = Color.Success; });
            }
            catch (Exception e)
            {
                ErrorHandler.ProcessError(e);
            }
        }
    }
}
