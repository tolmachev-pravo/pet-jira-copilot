using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Clipboard
{
    public class Clipboard : IClipboard
    {
        private readonly IJSRuntime _jsRuntime;

        public Clipboard(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public ValueTask CopyToAsync(string text)
        {
            return _jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
        }
    }
}
