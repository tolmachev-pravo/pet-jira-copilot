using Pet.Jira.Application.Common.Mapping;
using Pet.Jira.Domain.Entities.Blog;
using System;

namespace Pet.Jira.Application.Blog.Dto
{
	public class ArticleDto : IMapFrom<Article>
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }
		public string ImageUrl { get; set; }
		public string Link { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
