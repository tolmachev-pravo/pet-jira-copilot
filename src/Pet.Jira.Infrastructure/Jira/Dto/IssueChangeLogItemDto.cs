namespace Pet.Jira.Infrastructure.Jira.Dto
{
    public class IssueChangeLogItemDto
    {
        public string FromId { get; set; }
        public string ToId { get; set; }
        public IssueChangeLogDto ChangeLog { get; set; }

        public bool ToInProgress => ToId == JiraConstants.Status.InProgress;
        public bool FromInProgress => FromId == JiraConstants.Status.InProgress;
    }
}
