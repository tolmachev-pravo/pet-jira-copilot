using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Markdown
{
	public interface IMarkdownService
	{
		Task<string> DownloadMarkdownAsync(string path);
	}
}
