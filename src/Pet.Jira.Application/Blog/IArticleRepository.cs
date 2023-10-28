using Pet.Jira.Application.Blog.Commands.CreateArticle;
using Pet.Jira.Domain.Entities.Blog;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Blog
{
    public interface IArticleRepository
    {
        Task<Article> AddAsync(CreateArticleCommand article, CancellationToken cancellationToken = default);
    }
}
