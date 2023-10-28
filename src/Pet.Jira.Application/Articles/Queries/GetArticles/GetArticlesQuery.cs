using MediatR;
using Pet.Jira.Application.Articles.Dto;
using System.Collections.Generic;

namespace Pet.Jira.Application.Articles.Queries.GetArticles
{
    public class GetArticlesQuery : IRequest<IEnumerable<ArticleDto>>
    {
    }
}