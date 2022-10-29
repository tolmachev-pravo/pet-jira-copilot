using System.IO;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Markdown
{
	public class MarkdownService : IMarkdownService
	{
		public async Task<string> DownloadMarkdownAsync(string path)
		{
            return await File.ReadAllTextAsync(path);
        }
	}
}
