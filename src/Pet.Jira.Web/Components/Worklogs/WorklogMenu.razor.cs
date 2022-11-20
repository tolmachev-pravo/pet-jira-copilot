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
            ClipboardItemElementCollection clipboardItemElements = new()
            {
                new ClipboardItemElement
                {
                    MimeType = ClipboardMimeType.Html,
                    Data = $"<b>IN REVIEW</b> {HtmlMainText()}"
                },
                new ClipboardItemElement
                {
                    MimeType = ClipboardMimeType.Plain,
                    Data = $"**IN REVIEW** {MarkdownMainText()}"
                }
            };
            await CopyToClipboardAsync(clipboardItemElements);
        }

        private async Task CopyToClipboardInProgressAsync()
        {
            ClipboardItemElementCollection clipboardItemElements = new()
            {
                new ClipboardItemElement
                {
                    MimeType = ClipboardMimeType.Html,
                    Data = $"<b>IN PROGRESS</b> {HtmlMainText()}"
                },
                new ClipboardItemElement
                {
                    MimeType = ClipboardMimeType.Plain,
                    Data = $"**IN PROGRESS** {MarkdownMainText()}"
                }
            };
            await CopyToClipboardAsync(clipboardItemElements);
        }

        private string MarkdownMainText() => $"[{Entity.Issue.Key}]({Entity.Issue.Link}) {Entity.Issue.Summary}";
        private string HtmlMainText() => $"<a href=\"{Entity.Issue.Link}\">{Entity.Issue.Key}</a> {Entity.Issue.Summary}";

        private async Task CopyToClipboardAsync(ClipboardItemElementCollection clipboardItemElements)
        {
            try
            {
                var isClipboardSupported = await _clipboard.IsSupportedAsync();
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
                    throw new NotSupportedException("Clipboard is not supported in this browser");
                }
            }        
            catch (Exception e)
            {
                ErrorHandler.ProcessError(e);
            }
        }
    }
}
