using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pet.Jira.Application.Worklogs.Queries;
using Pet.Jira.Domain.Models.Worklogs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Worklogs
{
    public partial class WorklogListPage : ComponentBase
    {
        private readonly ComponentModel Model = ComponentModel.Create();

        [Inject] private IMediator Mediator { get; set; }
        [Inject] private ISnackbar Snackbar { get; set; }

        protected async Task SearchAsync(GetDailyWorklogSummaries.Query filter)
        {
            try
            {
                Model.State = ComponentModelState.InProgress;
                var filterResult = await Mediator.Send(filter);
                Model.ListItems = filterResult.Worklogs;
            }
            catch (Exception e)
            {
                Snackbar.Add(
                    e.Message,
                    Severity.Error,
                    config => { config.ActionColor = Color.Error; });
            }
            finally
            {
                Model.State = ComponentModelState.Success;
            }
        }

        private class ComponentModel
        {
            public static ComponentModel Create()
            {
                return new ComponentModel();
            }

            public IList<DailyWorklogSummary> ListItems { get; set; }
            public ComponentModelState State { get; set; } = ComponentModelState.Unknown;
            public bool InProgress => State == ComponentModelState.InProgress;
        }
    }
}
