using MediatR;
using Microsoft.AspNetCore.Components;
using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Application.Worklogs.Queries;
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

        protected async Task SearchAsync(GetWorklogCollection.Query filter)
        {
            try
            {
                Model.StateTo(ComponentModelState.InProgress);
                var filterResult = await Mediator.Send(filter);
                Model.Items = filterResult.WorklogCollection.Days;
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

            public IEnumerable<WorklogCollectionDay> Items { get; set; }
        }
    }
}
