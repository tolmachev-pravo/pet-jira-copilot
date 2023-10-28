using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pet.Jira.Application.Blog.Commands.CreateArticle;
using Pet.Jira.Application.Blog.Queries.GetArticles;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ArticlesController : ControllerBase
	{
		private readonly IMediator _mediator;

		public ArticlesController(
			IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost]
		public async Task<ActionResult<object>> Create(
			CreateArticleCommand article,
			CancellationToken cancellationToken = default)
		{
			var articleId = await _mediator.Send(article, cancellationToken);
			var articles = await _mediator.Send(new GetArticlesQuery(), cancellationToken);
			return articles.First(entity => entity.Id == articleId);
		}
	}
}
