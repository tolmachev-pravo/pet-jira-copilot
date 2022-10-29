using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Pet.Jira.Web.Components.Common
{
    public partial class Stub : ComponentBase
    {
        [Parameter] public Color Color { get; set; } = Color.Default;
        [Parameter] public string Icon { get; set; }
        [Parameter] public string Message { get; set; }
    }
}
