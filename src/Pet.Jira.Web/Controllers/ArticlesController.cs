using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pet.Jira.Application.Articles.Commands.CreateArticle;
using Pet.Jira.Application.Articles.Commands.DeleteArticle;
using Pet.Jira.Application.Articles.Queries.GetArticles;
using System;
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

        [HttpGet]
        public async Task<ActionResult<object>> GetAll(
            CancellationToken cancellationToken = default)
        {
            var articles = await _mediator.Send(new GetArticlesQuery(), cancellationToken);
            return Ok(articles);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var deleted = await _mediator.Send(new DeleteArticleCommand { Id = id }, cancellationToken);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
