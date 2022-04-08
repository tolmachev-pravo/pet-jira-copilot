using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pet.Jira.Application.Worklogs.Queries;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Worklogs
{
    public partial class WorklogFilter : ComponentBase
    {
        private readonly ComponentModel Model = ComponentModel.Create();
        private const string FilterCacheItemName = "WorklogFilter";

        [Parameter] public EventCallback<GetDailyWorklogSummaries.Query> OnSearchPressed { get; set; }
        [Inject] private ILocalStorageService LocalStorage { get; set; }

        protected async Task Search()
        {
            await SaveFilterCache();
            await OnSearchPressed.InvokeAsync(new GetDailyWorklogSummaries.Query()
            {
                StartDate = Model.Filter.StartDate.Value,
                EndDate = Model.Filter.EndDate.Value.AddDays(1).AddMinutes(-1),
                DailyWorkingStartTime = Model.Filter.DailyWorkingStartTime.Value,
                DailyWorkingEndTime = Model.Filter.DailyWorkingEndTime.Value
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

        public class ComponentModel
        {
            public static ComponentModel Create()
            {
                return new ComponentModel
                {
                    Filter = new FilterModel()
                };
            }

            public FilterModel Filter { get; set; }
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
        }
    }
}
