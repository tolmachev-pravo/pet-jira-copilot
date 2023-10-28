using Pet.Jira.Application.Blog.Dto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Blog
{
	public interface IArticleDataSource
	{
		Task<IEnumerable<ArticleDto>> GetArticlesAsync(CancellationToken cancellationToken = default);
	}
}
