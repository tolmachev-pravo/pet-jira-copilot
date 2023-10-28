using Microsoft.AspNetCore.Components;
using Pet.Jira.Application.Blog.Dto;

namespace Pet.Jira.Web.Components.Project.Articles
{
    public partial class Article : ComponentBase
    {
        [Parameter] public ArticleDto Entity { get; set; }
    }
}
