using Microsoft.AspNetCore.Components;
using Pet.Jira.Application.Worklogs.Dto;

namespace Pet.Jira.Web.Components.Worklogs
{
    public partial class ActualWorklogItem : ComponentBase
    {
        [Parameter] public WorklogCollectionItem Entity { get; set; }
    }
}
