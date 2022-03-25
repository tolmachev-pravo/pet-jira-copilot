namespace Pet.Jira.Infrastructure.Jira
{
    public class JiraConfiguration : IJiraConfiguration
    {
        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
