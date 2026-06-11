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

        [Test]
        public void Plan_TasksOutOfOrder_PackedInRealStartOrder()
        {
            var late = Task(new DateTime(2026, 6, 1, 16, 0, 0), new DateTime(2026, 6, 1, 17, 0, 0));  // 1h
            var early = Task(new DateTime(2026, 6, 1, 9, 0, 0), new DateTime(2026, 6, 1, 12, 0, 0));   // 3h
            var events = new List<Event> { late, early };

            var result = _sut.Plan(_day, events);

            Assert.That(result[0].Event, Is.SameAs(early));
            Assert.That(result[1].Event, Is.SameAs(late));
            Assert.That(result[0].Duration, Is.EqualTo(TimeSpan.FromHours(6)));  // 3h weight -> 6h
            Assert.That(result[1].Duration, Is.EqualTo(TimeSpan.FromHours(2)));  // 1h weight -> 2h
        }

        [Test]
        public void Plan_CalendarPlusTask_CalendarExact_TaskFillsRemainder()
        {
            var meeting = Calendar(new DateTime(2026, 6, 1, 11, 0, 0), new DateTime(2026, 6, 1, 13, 0, 0)); // 2h
            var task = Task(new DateTime(2026, 6, 1, 9, 0, 0), new DateTime(2026, 6, 1, 9, 30, 0));         // 0.5h
            var events = new List<Event> { meeting, task };

            var result = _sut.Plan(_day, events);

            var calendarSlot = result.Single(p => p.Event == meeting);
            Assert.That(calendarSlot.Start, Is.EqualTo(meeting.Start));
            Assert.That(calendarSlot.End, Is.EqualTo(meeting.End));

            var taskSlot = result.Single(p => p.Event == task);
            Assert.That(taskSlot.Duration, Is.EqualTo(TimeSpan.FromHours(6))); // 8h - 2h
            Assert.That(taskSlot.Start, Is.EqualTo(new DateTime(2026, 6, 1, 10, 0, 0)));

            Assert.That(result.Sum(p => p.Duration.Ticks), Is.EqualTo(TimeSpan.FromHours(8).Ticks));
        }

        [Test]
        public void Plan_CalendarFillsEightHours_TasksGetNothing()
        {
            var meeting = Calendar(new DateTime(2026, 6, 1, 10, 0, 0), new DateTime(2026, 6, 1, 18, 0, 0)); // 8h
            var task = Task(new DateTime(2026, 6, 1, 9, 0, 0), new DateTime(2026, 6, 1, 10, 0, 0));
            var events = new List<Event> { meeting, task };

            var result = _sut.Plan(_day, events);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Single().Event, Is.SameAs(meeting));
        }

        [Test]
        public void Plan_CalendarExceedsEightHours_TasksGetNothing_NoNegative()
        {
            var meeting = Calendar(new DateTime(2026, 6, 1, 9, 0, 0), new DateTime(2026, 6, 1, 18, 0, 0)); // 9h
            var task = Task(new DateTime(2026, 6, 1, 9, 0, 0), new DateTime(2026, 6, 1, 10, 0, 0));
            var events = new List<Event> { meeting, task };

            var result = _sut.Plan(_day, events);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Single().Event, Is.SameAs(meeting));
        }

        [Test]
        public void Plan_NoEvents_ReturnsEmpty()
        {
            var result = _sut.Plan(_day, new List<Event>());
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Plan_MultipleCalendarEvents_BudgetSubtractsTheirSum()
        {
            var morning = Calendar(new DateTime(2026, 6, 1, 9, 0, 0), new DateTime(2026, 6, 1, 10, 0, 0));   // 1h
            var afternoon = Calendar(new DateTime(2026, 6, 1, 14, 0, 0), new DateTime(2026, 6, 1, 15, 30, 0)); // 1.5h
            var task = Task(new DateTime(2026, 6, 1, 11, 0, 0), new DateTime(2026, 6, 1, 11, 30, 0));
            var events = new List<Event> { morning, afternoon, task };

            var result = _sut.Plan(_day, events);

            var taskSlot = result.Single(p => p.Event == task);
            Assert.That(taskSlot.Duration, Is.EqualTo(TimeSpan.FromHours(5.5))); // 8h - (1h + 1.5h)
            Assert.That(result.Sum(p => p.Duration.Ticks), Is.EqualTo(TimeSpan.FromHours(8).Ticks));
        }

        [Test]
        public void Plan_ThreeUnequalTasks_SumIsExactlyEightHours()
        {
            var t1 = Task(new DateTime(2026, 6, 1, 9, 0, 0), new DateTime(2026, 6, 1, 10, 0, 0));   // 1h
            var t2 = Task(new DateTime(2026, 6, 1, 10, 0, 0), new DateTime(2026, 6, 1, 12, 0, 0));  // 2h
            var t3 = Task(new DateTime(2026, 6, 1, 12, 0, 0), new DateTime(2026, 6, 1, 16, 0, 0));  // 4h
            var events = new List<Event> { t1, t2, t3 };

            var result = _sut.Plan(_day, events);

            Assert.That(result, Has.Count.EqualTo(3));
            // 1h/2h/4h weights of an 8h budget produce non-integer tick slices;
            // the last-task-remainder logic must still make the sum exact.
            Assert.That(result.Sum(p => p.Duration.Ticks), Is.EqualTo(TimeSpan.FromHours(8).Ticks));
        }
    }
}
