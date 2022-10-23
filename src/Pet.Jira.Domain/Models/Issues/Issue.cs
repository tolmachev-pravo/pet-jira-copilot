using Pet.Jira.Domain.Models.Abstract;

namespace Pet.Jira.Domain.Models.Issues
{
    public class Issue : IIssue, IEntity<string>
    {
        public string Key { get; set; }
        public string Summary { get; set; }
        public string Link { get; set; }
    }
}
