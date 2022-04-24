using Pet.Jira.Domain.Models.Issues;

namespace Pet.Jira.Infrastructure.Jira
{
    public static class JiraConstants
    {
        public static class Status
        {
            public const string FieldName = "status";
            public static IssueStatus Default => new() { Id = "3", Name = "In Progress" };
        }
    }
}
