using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pet.Jira.Application.Issues;
using Pet.Jira.Application.Storage;
using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Domain.Models.Issues;
using Pet.Jira.Infrastructure.Jira;
using Pet.Jira.Web.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Worklogs
{
    public partial class WorklogDayItemDialog : ComponentBase
    {
        private readonly ComponentModel _model = ComponentModel.Create();

        [Parameter] public WorkingDay WorkingDay { get; set; }
        [Parameter] public Color Color { get; set; } = Color.Default;
        [Parameter] public string Icon { get; set; } = Icons.Material.Filled.MoreVert;
        [Parameter] public string Label { get; set; } = "Default";

        [CascadingParameter] public ErrorHandler ErrorHandler { get; set; }
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Inject] private IMemoryCache<string, Issue> IssueCache { get; set; }
		[Inject] private IIssueDataSource IssueDataSource { get; set; }

		public class ComponentModel
        {
            public static ComponentModel Create()
            {
                return new ComponentModel();
            }

            public WorklogModel Worklog { get; set; } = new WorklogModel();
        }

        public class WorklogModel
        {
            [Required]
            public DateTime? Date { get; set; }

            [Required]
            public TimeSpan? StartTime { get; set; }

            [Required]
            public TimeSpan? CompleteTime { get; set; }

            [Required]
            public string Comment { get; set; }

            [Required]
            public Issue Issue { get; set; }

            public bool IsInitialized { get; set; }

            public void Initialize(WorkingDay workingDay)
            {
                if (workingDay != null)
                {
                    Date = workingDay.Date;
                    StartTime = workingDay.Settings.WorkingStartTime;
                    CompleteTime = workingDay.Settings.WorkingStartTime.Add(new TimeSpan(0,15,0));
                }
            }

            public WorkingDayWorklog Convert()
            {
                return new WorkingDayWorklog
                {
                    StartDate = Date.Value.Add(StartTime.Value),
                    CompleteDate = Date.Value.Add(CompleteTime.Value),
                    RawStartDate = Date.Value.Add(StartTime.Value),
                    RawCompleteDate = Date.Value.Add(CompleteTime.Value),
                    RemainingTimeSpent = CompleteTime.Value - StartTime.Value,
                    Issue = Issue,
                    Type = Domain.Models.Worklogs.WorklogType.Actual,
                    Source = Domain.Models.Worklogs.WorklogSource.Calendar,
                    Comment = Comment
                };
            }
        }

        protected override void OnInitialized()
        {
            _model.Worklog.Initialize(WorkingDay);
        }

        private async Task<IEnumerable<Issue>> SearchIssuesAsync(string value)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(value)
					&& value.IsJiraKey())
                {
                    var issue = await IssueDataSource.GetIssueAsync(value);
                    return issue is null
                        ? Array.Empty<Issue>()
                        : new[] { issue };
				}
                else
                {
					var issues = await IssueCache.GetValuesAsync();
					return issues.Values.OrderBy(value => value.Identifier);
				}
            }
            catch (Exception e)
            {
                ErrorHandler.ProcessError(e);
                return _model.Worklog.Issue is null
                    ? Array.Empty<Issue>()
                    : new[] { _model.Worklog.Issue };
            }
        }

        void Submit()
        {
            var worklog = _model.Worklog.Convert();
            var result = DialogResult.Ok(worklog, typeof(WorkingDayWorklog));
            MudDialog.Close(result);

        }
        void Cancel() => MudDialog.Cancel();
    }
}
