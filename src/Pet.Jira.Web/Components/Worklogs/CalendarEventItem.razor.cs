using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pet.Jira.Application.Extensions.YandexCalendar.Dto;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Worklogs
{
    public partial class CalendarEventItem : ComponentBase
    {
        [Parameter] public YandexCalendarEventDto Entity { get; set; } = default!;
        [Parameter] public bool IsLogged { get; set; }
        [Parameter] public EventCallback<YandexCalendarEventDto> OnLogPressed { get; set; }
    }
}
