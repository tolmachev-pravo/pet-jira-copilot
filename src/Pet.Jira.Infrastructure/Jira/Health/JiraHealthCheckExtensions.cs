using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Pet.Jira.Infrastructure.Jira.Health
{
    public static class JiraHealthCheckExtensions
    {
        public static void AddJiraHealthCheck(this IHealthChecksBuilder builder)
        {
            builder.AddCheck<JiraHealthCheck>(
                    JiraHealthCheck.Name,
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "jira", "network" });
        }
    }
}
