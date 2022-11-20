using Microsoft.JSInterop;
using Pet.Jira.Application.Extensions;
using System.Linq;
using System.Threading.Tasks;
using Thinktecture.Blazor.AsyncClipboard;
using Thinktecture.Blazor.AsyncClipboard.Models;

namespace Pet.Jira.Web.Components.Clipboard
{
    public class Clipboard : IClipboard
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly AsyncClipboardService _asyncClipboardService;

        public Clipboard(IJSRuntime jsRuntime,
            AsyncClipboardService asyncClipboardService)
        {
            _jsRuntime = jsRuntime;
            _asyncClipboardService = asyncClipboardService;
        }

        public ValueTask DisposeAsync()
        {
            return _asyncClipboardService.DisposeAsync();
        }

        /// <summary>
        /// Write to clipboard <see cref="ClipboardItemElementCollection"/>
        /// </summary>
        /// <param name="clipboardItemElements"></param>
        /// <returns></returns>
        public async Task WriteAsync(ClipboardItemElementCollection clipboardItemElements)
        {
            var jsClipboard = await _jsRuntime.InvokeAsync<IJSObjectReference>("clipboard");

            var elements = await clipboardItemElements.ToDictionaryAsync(
                element => element.MimeType.ToEnumString(),
                element => jsClipboard.InvokeAsync<IJSObjectReference>("convertToBlob", element.Data, element.MimeType));

            var items = new[]
            {
                new ClipboardItem(elements, new ClipboardItemOptions { PresentationStyle = PresentationStyle.Inline })
            };
            
            await _asyncClipboardService.WriteAsync(items);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsSupportedAsync()
        {
            try
            {
                return await _asyncClipboardService.IsSupportedAsync();
            }
            catch
            {
                return false;
            }            
        }
    }
}
