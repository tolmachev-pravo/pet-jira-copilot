using System;
using System.Collections.Concurrent;

namespace Pet.Jira.Application.Tracing
{
    public static class PerformanceTraceManager
    {
        private static readonly ConcurrentDictionary<string, IPerformanceTracer> PerformanceTracers =
            new ConcurrentDictionary<string, IPerformanceTracer>();

        private static Func<string, IPerformanceTracer> _tracerFactory;

        static PerformanceTraceManager()
        {
            _tracerFactory = name => new PerformanceTracer(name);
        }

        public static IPerformanceTracer GetTracer(string tracerName)
        {
            return PerformanceTracers.GetOrAdd(tracerName, _tracerFactory);
        }

        public static T GetTracer<T>(string tracerName)
            where T : IPerformanceTracer
        {
            return (T)GetTracer(tracerName);
        }

        public static void Initialize(Func<string, IPerformanceTracer> tracerFactory)
        {
            _tracerFactory = tracerFactory;
        }
    }
}
