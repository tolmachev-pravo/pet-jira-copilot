using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Pet.Jira.Application.Articles;
using Pet.Jira.Application.Articles.Dto;
using Pet.Jira.Infrastructure.Data.Contexts;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Articles
{
    public class ArticleDataSource : IArticleDataSource
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public ArticleDataSource(
            ApplicationDbContext dbContext,
            IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ArticleDto>> GetArticlesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Articles
                .ProjectTo<ArticleDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
