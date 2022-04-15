using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Components;
using Pet.Jira.Application.Tracing;
using Pet.Jira.Application.Worklogs.Queries;

namespace Pet.Jira.Web.Components.Debugging
{
    public partial class Debug : ComponentBase
    {
        private readonly PerformanceTracer _performanceTracer = PerformanceTraceManager.GetTracer<PerformanceTracer>(nameof(GetDailyWorklogSummaries));

        public string Body => GetBody();

        public string GetBody()
        {
            var stringBuilder = new StringBuilder();
            IEnumerable<Measure> measures = _performanceTracer.Measures;
            stringBuilder.AppendLine(Measure.Headers);

            foreach (var measure in measures)
            {
                stringBuilder.AppendLine(measure.ToString());
            }

            return stringBuilder.ToString();
        }
    }
}
