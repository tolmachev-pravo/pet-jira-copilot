using MediatR;
using System;

namespace Pet.Jira.Application.Articles.Commands.DeleteArticle
{
    public class DeleteArticleCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }
}
