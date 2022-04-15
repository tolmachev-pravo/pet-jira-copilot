using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace Pet.Jira.Application.Tracing
{
    public class PerformanceTracer : IPerformanceTracer
    {
        private readonly ConcurrentBag<TracingScope> _scopesPool;
        private readonly ConcurrentDictionary<string, Measure> _measures;

        public PerformanceTracer(string name)
        {
            Name = name;
            _scopesPool = new ConcurrentBag<TracingScope>();
            _measures = new ConcurrentDictionary<string, Measure>(StringComparer.InvariantCultureIgnoreCase);
        }

        public string Name { get; }

        public ICollection<Measure> Measures => _measures.Values;

        public IDisposable Trace(string category)
        {
            var measure = _measures.GetOrAdd(category, c => new Measure(c));

            if (!_scopesPool.TryTake(out var scope))
            {
                scope = new TracingScope(this);
            }

            scope.Begin(measure);

            return scope;
        }

        private class TracingScope : IDisposable
        {
            private readonly PerformanceTracer _tracer;
            private readonly Stopwatch _stopwatch;
            private Measure _measure;

            public TracingScope(PerformanceTracer tracer)
            {
                _tracer = tracer;
                _stopwatch = new Stopwatch();
            }

            public void Begin(Measure measure)
            {
                _measure = measure;
                _stopwatch.Start();
            }

            public void Dispose()
            {
                _stopwatch.Stop();
                _measure.Update(_stopwatch);

                _stopwatch.Reset();
                _measure = null;

                _tracer._scopesPool.Add(this);
            }
        }
    }
}
