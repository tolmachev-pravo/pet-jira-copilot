using Microsoft.AspNetCore.Components;
using Pet.Jira.Application.Tracing;
using Pet.Jira.Application.Worklogs.Queries;
using System.Collections.Generic;
using System.Text;

namespace Pet.Jira.Web.Components.Debugging
{
    public partial class Debug : ComponentBase
    {
        private readonly PerformanceTracer _performanceTracer = PerformanceTraceManager.GetTracer<PerformanceTracer>(nameof(GetWorklogCollection));

        public string Body => GetBody();

        public string GetBody()
        {
            var stringBuilder = new StringBuilder();
            IEnumerable<Measure> measures = _performanceTracer.Measures;
            stringBuilder.AppendLine(Measure.Headers);
            stringBuilder.AppendLine(Measure.HeaderDelimeter);
            foreach (var measure in measures)
            {
                stringBuilder.AppendLine(measure.ToString());
            }

            return stringBuilder.ToString();
        }
    }
}
