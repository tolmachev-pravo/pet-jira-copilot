using System;

namespace Pet.Jira.Application.Tracing
{
    public interface IPerformanceTracer
    {
        IDisposable Trace(string category);
    }
}
