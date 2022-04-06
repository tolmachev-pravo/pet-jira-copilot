using System.Linq;

namespace Pet.Jira.Application.Extensions
{
    public static class UrlExtensions
    {
        public static string AppendUrl(this string url, params string[] segments)
        {
            return string.Join("/", new[] { url.TrimEnd('/') }
                .Concat(segments.Select(segment => segment.Trim('/'))));
        }
    }
}
