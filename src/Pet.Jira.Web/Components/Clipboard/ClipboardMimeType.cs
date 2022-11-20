using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Pet.Jira.Web.Components.Clipboard
{
    /// <summary>
    /// https://www.iana.org/assignments/media-types/media-types.xhtml
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverterWithAttributeSupport))]
    public enum ClipboardMimeType
    {
        [EnumMember(Value = "text/html")]
        Html,

        [EnumMember(Value = "text/plain")]
        Plain
    }
}
