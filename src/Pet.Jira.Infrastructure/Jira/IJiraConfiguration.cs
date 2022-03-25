namespace Pet.Jira.Infrastructure.Jira
{
    public interface IJiraConfiguration
    {
        string Url { get; set; }
        string Username { get; set; }
        string Password { get; set; }
    }
}