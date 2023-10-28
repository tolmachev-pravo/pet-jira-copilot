using Pet.Jira.Application.Blog;
using Pet.Jira.Application.Blog.Commands.CreateArticle;
using Pet.Jira.Domain.Entities.Blog;
using Pet.Jira.Infrastructure.Data.Contexts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Blog
{
    public class ArticleRepository : IArticleRepository
	{
		private readonly ApplicationDbContext _dbContext;

		public ArticleRepository(
			ApplicationDbContext dbContext) 
		{
			_dbContext = dbContext;
		}

		public async Task<Article> AddAsync(CreateArticleCommand article, CancellationToken cancellationToken = default)
		{
			var articleEntity = new Article
			{
				Title = article.Title,
				Content = article.Content,
				ImageUrl = article.ImageUrl,
				Link = article.Link,
				CreatedAt = DateTime.UtcNow
			};

			_dbContext.Articles.Add(articleEntity);
			await _dbContext.SaveChangesAsync(cancellationToken);

			return articleEntity;
		
		}
	}
}
