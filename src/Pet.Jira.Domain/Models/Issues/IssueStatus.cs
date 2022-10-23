using Pet.Jira.Domain.Models.Abstract;

namespace Pet.Jira.Domain.Models.Issues
{
    public class IssueStatus : IEntity<string>
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string Key => Id;
    }
}
