using Microsoft.AspNetCore.Components;
using Pet.Jira.Application.Articles.Dto;

namespace Pet.Jira.Web.Components.Project.Articles
{
    public partial class Article : ComponentBase
    {
        [Parameter] public ArticleDto Entity { get; set; }
    }
}
