using Blazored.LocalStorage;
using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pet.Jira.Application.Issues.Queries;
using Pet.Jira.Application.Worklogs.Queries;
using Pet.Jira.Domain.Models.Issues;
using Pet.Jira.Infrastructure.Jira;
using Pet.Jira.Web.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Worklogs
{
    public partial class WorklogFilter : ComponentBase
    {
        private readonly ComponentModel Model = ComponentModel.Create();
        private const string FilterCacheItemName = "WorklogFilter";

        [Parameter] public EventCallback<GetWorklogCollection.Query> OnSearchPressed { get; set; }
        [Inject] private ILocalStorageService LocalStorage { get; set; }
        [Inject] private IMediator Mediator { get; set; }
        [CascadingParameter] public ErrorHandler ErrorHandler { get; set; }

        protected async Task Search()
        {
            await SaveFilterCache();
            await OnSearchPressed.InvokeAsync(new GetWorklogCollection.Query()
            {
                StartDate = Model.Filter.StartDate.Value,
                EndDate = Model.Filter.EndDate.Value.AddDays(1).AddMinutes(-1),
                DailyWorkingStartTime = Model.Filter.DailyWorkingStartTime.Value,
                DailyWorkingEndTime = Model.Filter.DailyWorkingEndTime.Value,
                IssueStatusId = Model.Filter.IssueStatus.Id
            });
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await TryInitFilterModelByCacheAsync();
            }
            else
            {
                await SaveFilterCache();
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task SaveFilterCache()
        {
            await LocalStorage.SetItemAsync(FilterCacheItemName, Model.Filter);
        }

        private async Task TryInitFilterModelByCacheAsync()
        {
            var filterCache = await LocalStorage.GetItemAsync<FilterModel>(FilterCacheItemName);
            if (filterCache == null)
            {
                return;
            }

            Model.Filter = filterCache;
            StateHasChanged();
        }

        private async Task<IEnumerable<IssueStatus>> SearchIssueStatuses(string value)
        {
            try
            {
                var result = await Mediator.Send(new GetIssueStatuses.Query());

                if (string.IsNullOrEmpty(value)
                    || value.Equals(Model.Filter.IssueStatus?.Name, StringComparison.InvariantCultureIgnoreCase))
                    return result.IssueStatuses;
                return result.IssueStatuses
                    .Where(status => status.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase));
            }
            catch (Exception e)
            {
                ErrorHandler.ProcessError(e);
                return await Task.FromResult(Model.Filter.IssueStatus != null
                    ? new List<IssueStatus> { Model.Filter.IssueStatus }
                    : Enumerable.Empty<IssueStatus>());
            }
        }

        public class ComponentModel
        {
            public static ComponentModel Create()
            {
                return new ComponentModel();
            }

            public FilterModel Filter { get; set; } = new FilterModel();
        }

        public class FilterModel
        {
            [Required]
            [JsonIgnore]
            public DateRange DateRange { get; set; } = new DateRange(DateTime.Now.AddDays(-7).Date, DateTime.Now.Date);

            [Required] 
            public DateTime? StartDate => DateRange.Start;

            [Required] 
            public DateTime? EndDate => DateRange.End;

            [Required] 
            public TimeSpan? DailyWorkingStartTime { get; set; } = TimeSpan.FromHours(10);

            [Required] 
            public TimeSpan? DailyWorkingEndTime { get; set; } = TimeSpan.FromHours(19);

            [Required]
            public IssueStatus IssueStatus { get; set; } = JiraConstants.Status.Default;
        }
    }
}
