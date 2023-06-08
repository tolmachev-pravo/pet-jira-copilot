using Microsoft.AspNetCore.Components;
using Pet.Jira.Application.Worklogs.Dto;
using System.Collections.Generic;

namespace Pet.Jira.Web.Components.Worklogs
{
    public partial class WorklogList : ComponentBase
    {
        private readonly ComponentModel Model = ComponentModel.Create();

        [Parameter] public IEnumerable<WorkingDay> Items { get; set; }

        private class ComponentModel
        {
            public static ComponentModel Create()
            {
                return new ComponentModel();
            }
        }
    }
}
