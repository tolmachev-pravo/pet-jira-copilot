using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Clipboard
{
    public interface IClipboard
    {
        ValueTask CopyToAsync(string text);
    }
}
