namespace Pet.Jira.Infrastructure.Jira.Query
{
    public class JiraQueryFactory : IJiraQueryFactory
    {
        public JiraQuery Create()
        {
            return new JiraQuery();
        }
    }
}
