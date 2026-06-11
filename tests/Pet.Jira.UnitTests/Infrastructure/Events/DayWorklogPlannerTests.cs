using NUnit.Framework;
using Pet.Jira.Application.Events;
using Pet.Jira.Domain.Models.Events;
using Pet.Jira.Infrastructure.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pet.Jira.UnitTests.Infrastructure.Events
{
    [TestFixture]
    public class DayWorklogPlannerTests
    {
        private readonly DateOnly _day = new(2026, 6, 1);
        private IDayWorklogPlanner _sut;

        [SetUp]
        public void Setup() => _sut = new DayWorklogPlanner();

        private Event Task(DateTime start, DateTime end) =>
            new(start, end, "task", null, null, null, "PROJ-1", EventSource.Task);

        private Event Calendar(DateTime start, DateTime end) =>
            new(start, end, "meeting", null, null, null, null, EventSource.Calendar);

        [Test]
        public void Plan_TwoEqualTasks_SplitEightHoursEqually_PackedFromTen()
        {
            var events = new List<Event>
            {
                Task(new DateTime(2026, 6, 1, 9, 0, 0), new DateTime(2026, 6, 1, 11, 0, 0)),  // 2h
                Task(new DateTime(2026, 6, 1, 15, 0, 0), new DateTime(2026, 6, 1, 17, 0, 0)), // 2h
            };

            var result = _sut.Plan(_day, events);

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Sum(p => p.Duration.Ticks), Is.EqualTo(TimeSpan.FromHours(8).Ticks));
            Assert.That(result[0].Duration, Is.EqualTo(TimeSpan.FromHours(4)));
            Assert.That(result[1].Duration, Is.EqualTo(TimeSpan.FromHours(4)));
            Assert.That(result[0].Start, Is.EqualTo(new DateTime(2026, 6, 1, 10, 0, 0)));
            Assert.That(result[1].Start, Is.EqualTo(result[0].End));
        }
    }
}
