using MediatR;
using Pet.Jira.Application.Blog.Dto;
using System.Collections.Generic;

namespace Pet.Jira.Application.Blog.Queries.GetArticles
{
    public class GetArticlesQuery : IRequest<IEnumerable<ArticleDto>>
    {
    }
}
