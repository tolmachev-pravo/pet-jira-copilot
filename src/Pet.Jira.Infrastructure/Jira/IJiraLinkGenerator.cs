namespace Pet.Jira.Infrastructure.Jira
{
    public interface IJiraLinkGenerator
    {
        string Generate(string issueKey);
    }
}
