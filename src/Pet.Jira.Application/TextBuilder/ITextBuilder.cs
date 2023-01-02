namespace Pet.Jira.Application.TextBuilder
{
    public interface ITextBuilder
    {
        public ITextBuilder AddText(string text, params TextOption[] options);
        public ITextBuilder AddLink(string href, string value);
        public ITextBuilder BeginOption(TextOption option);
        public ITextBuilder EndOption(TextOption option);
        public string Build();
    }
}
