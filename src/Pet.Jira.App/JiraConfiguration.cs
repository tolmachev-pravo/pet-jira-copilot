using Microsoft.Extensions.Configuration;
using Pet.Jira.Adapter;

namespace Pet.Jira.App
{
    public class JiraConfiguration : IJiraConfiguration
    {
        public string Url { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public JiraConfiguration(IConfiguration config)
        {
            UserName = config["Jira:UserName"];
            Password = config["Jira:Password"];
            Url = config["Jira:Url"];
        }
    }
}
