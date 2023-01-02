using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pet.Jira.Web.Common;

namespace Pet.Jira.Web.Components.Common
{
    public partial class Stub : ComponentBase
    {
        [Parameter] public Color Color { get; set; } = Color.Default;
        [Parameter] public string Icon { get; set; } = WebConstants.Icons.Favicon;
        [Parameter] public string Message { get; set; }
        [Parameter] public string ViewBox { get; set; } = "0 0 260 260";
    }
}
