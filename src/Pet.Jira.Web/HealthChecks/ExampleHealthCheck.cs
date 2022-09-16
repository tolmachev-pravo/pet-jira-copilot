using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Pet.Jira.Web.HealthChecks
{
    public class ExampleHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var healthCheckResultHealthy = true;

            if (healthCheckResultHealthy)
            {
                return Task.FromResult(
                    HealthCheckResult.Healthy("A healthy result.", new Dictionary<string, object>
                    {
                        {"time", DateTime.Now},
                        {"count", Int32.MaxValue},
                        {"text", "IServiceCollection"}
                    }));
            }

            return Task.FromResult(
                HealthCheckResult.Unhealthy("An unhealthy result."));
        }
    }
}
