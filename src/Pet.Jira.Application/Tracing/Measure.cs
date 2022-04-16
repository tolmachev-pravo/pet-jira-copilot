using System;
using System.Diagnostics;

namespace Pet.Jira.Application.Tracing
{
    public class Measure
    {
        private readonly object _syncRoot;

        public Measure(string category)
        {
            Category = category;
            Sum = TimeSpan.Zero;
            Max = TimeSpan.Zero;
            Min = TimeSpan.MaxValue;
            Count = 0;

            _syncRoot = new object();
        }

        public string Category { get; }
        public TimeSpan Sum { get; private set; }
        public TimeSpan Max { get; private set; }
        public TimeSpan Min { get; private set; }
        public int Count { get; private set; }

        public TimeSpan Average => new(Sum.Ticks / Count);

        public void Update(Stopwatch stopwatch)
        {
            lock (_syncRoot)
            {
                Count++;

                Sum += stopwatch.Elapsed;

                if (stopwatch.Elapsed > Max)
                {
                    Max = stopwatch.Elapsed;
                }

                if (stopwatch.Elapsed < Min)
                {
                    Min = stopwatch.Elapsed;
                }
            }
        }

        private const string Format = "|{0,-40}|{1,18}|{2,18}|{3,18}|{4,7}|{5,18}|";

        public override string ToString() => String.Format(Format, Category, Sum, Max, Min, Count, Average);

        public static readonly string Headers = String.Format(Format, nameof(Category), nameof(Sum), nameof(Max), nameof(Min), nameof(Count), nameof(Average));

        public static readonly string HeaderDelimeter = String.Format(Format, "-", "-", "-", "-", "-", "-");
    }
}
