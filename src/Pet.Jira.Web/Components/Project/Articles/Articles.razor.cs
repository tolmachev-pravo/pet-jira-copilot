using MediatR;
using Microsoft.AspNetCore.Components;
using Pet.Jira.Application.Articles.Dto;
using Pet.Jira.Application.Articles.Queries.GetArticles;
using Pet.Jira.Web.Components.Common;
using Pet.Jira.Web.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Project.Articles
{
    public partial class Articles : ComponentBase
    {
        private readonly ComponentModel _model = ComponentModel.Create();

        [Inject] private IMediator Mediator { get; set; }
        [CascadingParameter] public ErrorHandler ErrorHandler { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var entities = await Mediator.Send(new GetArticlesQuery());
            _model.Entities = entities.OrderByDescending(entity => entity.CreatedAt);
        }

        private class ComponentModel : BaseStateComponentModel
        {
            public static ComponentModel Create()
            {
                return new ComponentModel();
            }

            public IEnumerable<ArticleDto> Entities { get; set; }
        }
    }
}
