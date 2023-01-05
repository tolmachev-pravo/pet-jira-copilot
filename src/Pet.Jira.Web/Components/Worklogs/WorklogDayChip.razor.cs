using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Pet.Jira.Web.Components.Worklogs
{
    public partial class WorklogDayChip : ComponentBase
    {
        [Parameter] public Color Color { get; set; } = Color.Default;
        [Parameter] public string Caption { get; set; }
        [Parameter] public string Value { get; set; }
    }
}
