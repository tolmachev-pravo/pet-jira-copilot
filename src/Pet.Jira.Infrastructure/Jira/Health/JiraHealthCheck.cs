using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Pet.Jira.Infrastructure.Jira.Health
{
    public class JiraHealthCheck : IHealthCheck
    {
        private readonly IJiraConfiguration _jiraConfiguration;

        public JiraHealthCheck(
            IOptions<JiraConfiguration> jiraConfiguration)
        {
            _jiraConfiguration = jiraConfiguration.Value;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var healthCheckResultHealthy = await PingAsync(cancellationToken);

            if (healthCheckResultHealthy)
            {
                return 
                    HealthCheckResult.Healthy("Healthy", new Dictionary<string, object>
                    {
                        {"Url", _jiraConfiguration.Url}
                    });
            }

            return HealthCheckResult.Unhealthy("Unhealthy");
        }

        public async Task<bool> PingAsync(CancellationToken cancellationToken = default)
        {
            HttpClient httpClient = new HttpClient();
            var content = await httpClient.GetAsync(_jiraConfiguration.Url, cancellationToken);
            return content.StatusCode == HttpStatusCode.OK;
        }
    }
}
