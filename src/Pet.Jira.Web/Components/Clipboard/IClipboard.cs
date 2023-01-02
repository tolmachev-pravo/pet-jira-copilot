using System;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Clipboard
{
    public interface IClipboard
    {
        Task WriteAsync(ClipboardItemElementCollection clipboardItemElements);
        Task<bool> IsSupportedAsync();
    }
}
