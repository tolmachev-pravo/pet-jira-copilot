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

        [Inject] private IClipboard _clipboard { get; set; }
        [Inject] private ISnackbar _snackbar { get; set; }

        private async Task CopyToClipboardInReviewAsync(WorklogCollectionItem entity)
        {
            ClipboardItemElementCollection clipboardItemElements = new()
            {
                new ClipboardItemElement
                {
                    MimeType = ClipboardMimeType.Html,
                    Data = $"<b>IN REVIEW</b> <a href=\"{entity.Issue.Link}\">{entity.Issue.Key}</a> {entity.Issue.Summary}"
                },
                new ClipboardItemElement
                {
                    MimeType = ClipboardMimeType.Plain,
                    Data = $"**IN REVIEW** [{entity.Issue.Key}]({entity.Issue.Link}) {entity.Issue.Summary}"
                }
            };
            await CopyToClipboardAsync(clipboardItemElements);
        }

        private async Task CopyToClipboardInProgressAsync(WorklogCollectionItem entity)
        {
            ClipboardItemElementCollection clipboardItemElements = new()
            {
                new ClipboardItemElement
                {
                    MimeType = ClipboardMimeType.Html,
                    Data = $"<b>IN PROGRESS</b> <a href=\"{entity.Issue.Link}\">{entity.Issue.Key}</a> {entity.Issue.Summary}"
                },
                new ClipboardItemElement
                {
                    MimeType = ClipboardMimeType.Plain,
                    Data = $"**IN PROGRESS** [{entity.Issue.Key}]({entity.Issue.Link}) {entity.Issue.Summary}"
                }
            };
            await CopyToClipboardAsync(clipboardItemElements);
        }

        private async Task CopyToClipboardAsync(ClipboardItemElementCollection clipboardItemElements)
        {
            var isClipboardSupported = !await _clipboard.IsSupportedAsync();
            if (isClipboardSupported)
            {
                await _clipboard.WriteAsync(clipboardItemElements);
                _snackbar.Add(
                    $"Copied to clipboard",
                    Severity.Success,
                    config => { config.ActionColor = Color.Success; });
            }
            else
            {
                throw new Exception("Clipboard is not supported in this browser");
            }
        }
    }
}
