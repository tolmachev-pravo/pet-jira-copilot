namespace Pet.Jira.Domain.Models.Issues
{
    public interface IIssue
    {
        string Key { get; set; }
        string Summary { get; set; }
        string Link { get; set; }
    }
}
