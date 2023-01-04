using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pet.Jira.Application.Issues.Queries;
using Pet.Jira.Application.TextBuilder;
using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Infrastructure.TextBuilder;
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
        [Parameter] public string Label { get; set; } = "Default";

        [CascadingParameter] public ErrorHandler ErrorHandler { get; set; }

        [Inject] private IClipboard _clipboard { get; set; }
        [Inject] private ISnackbar _snackbar { get; set; }
        [Inject] private IMediator Mediator { get; set; }

        private string PullRequestUrl { get; set; }

        private async Task CopyToClipboardInReviewAsync()
        {
            await InitPullRequestUrl();
            var clipboardItemElements = BuildClipboardElementCollection(InReviewText);
            await CopyToClipboardAsync(clipboardItemElements);
        }

        private async Task CopyToClipboardInProgressAsync()
        {
            var clipboardItemElements = BuildClipboardElementCollection(InProgressText);
            await CopyToClipboardAsync(clipboardItemElements);
        }

        private async Task InitPullRequestUrl()
        {
            var result = await Mediator.Send(new GetIssueOpenPullRequestUrl.Query { Identifier = Entity.Issue.Identifier });
            PullRequestUrl = result.Url;
        }

        private static ClipboardItemElementCollection BuildClipboardElementCollection(Func<ITextBuilder, string> buildText)
        {
            return new ClipboardItemElementCollection
            {
                new ClipboardItemElement
                {
                    MimeType = ClipboardMimeType.Html,
                    Data = buildText(new HtmlTextBuilder())
                },
                new ClipboardItemElement
                {
                    MimeType = ClipboardMimeType.Plain,
                    Data = buildText(new MarkdownTextBuilder())
                }
            };
        }

        private string InReviewText(ITextBuilder textBuilder)
        {
            textBuilder
               .AddText("IN REVIEW", TextOption.Bold)
               .AddLink(Entity.Issue.Link, Entity.Issue.Key)
               .AddText(Entity.Issue.Summary);

            if (!string.IsNullOrEmpty(PullRequestUrl))
            {
                textBuilder.AddLink(PullRequestUrl, PullRequestUrl);
            }
                
            return textBuilder.Build();
        }

        private string InProgressText(ITextBuilder textBuilder)
        {
            return textBuilder
                .AddText("IN PROGRESS", TextOption.Bold)
                .AddLink(Entity.Issue.Link, Entity.Issue.Key)
                .AddText(Entity.Issue.Summary)
                .Build();
        }

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
