namespace Pet.Jira.Domain.Models.Issues
{
    public class Issue : IIssue
    {
        public string Key { get; set; }
        public string Summary { get; set; }
        public string Link { get; set; }
    }
}
