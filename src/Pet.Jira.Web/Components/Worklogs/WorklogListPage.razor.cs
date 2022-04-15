using MediatR;
using Microsoft.AspNetCore.Components;
using Pet.Jira.Application.Worklogs.Queries;
using Pet.Jira.Domain.Models.Worklogs;
using Pet.Jira.Web.Components.Common;
using Pet.Jira.Web.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Worklogs
{
    public partial class WorklogListPage : ComponentBase
    {
        private readonly ComponentModel Model = ComponentModel.Create();

        [Inject] private IMediator Mediator { get; set; }
        [CascadingParameter] public ErrorHandler ErrorHandler { get; set; }

        protected async Task SearchAsync(GetDailyWorklogSummaries.Query filter)
        {
            try
            {
                Model.StateTo(ComponentModelState.InProgress);
                var filterResult = await Mediator.Send(filter);
                Model.ListItems = filterResult.Worklogs;
            }
            catch (Exception e)
            {
                ErrorHandler.ProcessError(e);
            }
            finally
            {
                Model.StateTo(ComponentModelState.Success);
            }
        }

        private class ComponentModel : BaseStateComponentModel
        {
            public static ComponentModel Create()
            {
                return new ComponentModel();
            }

            public IList<DailyWorklogSummary> ListItems { get; set; }
        }
    }
}
