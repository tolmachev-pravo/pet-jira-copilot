using Moq;
using NUnit.Framework;
using Pet.Jira.Application.Events;
using Pet.Jira.Domain.Models.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.UnitTests.Infrastructure.Events
{
    [TestFixture]
    public class EventAggregatorTests
    {
        private Mock<IEventDataSource> _source1Mock;
        private Mock<IEventDataSource> _source2Mock;
        private Pet.Jira.Infrastructure.Events.EventAggregator _sut;

        [SetUp]
        public void SetUp()
        {
            _source1Mock = new Mock<IEventDataSource>();
            _source2Mock = new Mock<IEventDataSource>();
            _sut = new Pet.Jira.Infrastructure.Events.EventAggregator(
                new[] { _source1Mock.Object, _source2Mock.Object });
        }

        [Test]
        public async Task GetEventsAsync_MultiDayEvent_AppearsOnEachDayItSpans()
        {
            var from = new DateOnly(2026, 6, 1);
            var to = new DateOnly(2026, 6, 3);
            var multiDayEvent = new Event(
                new DateTime(2026, 6, 1, 10, 0, 0),
                new DateTime(2026, 6, 3, 15, 0, 0),
                "Task A", null, null, null, EventSource.Task);

            _source1Mock
                .Setup(s => s.GetEventsAsync(from, to, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Event> { multiDayEvent });
            _source2Mock
                .Setup(s => s.GetEventsAsync(from, to, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Event>());

            var result = await _sut.GetEventsAsync(from, to, CancellationToken.None);

            Assert.That(result[new DateOnly(2026, 6, 1)], Contains.Item(multiDayEvent));
            Assert.That(result[new DateOnly(2026, 6, 2)], Contains.Item(multiDayEvent));
            Assert.That(result[new DateOnly(2026, 6, 3)], Contains.Item(multiDayEvent));
        }

        [Test]
        public async Task GetEventsAsync_MultipleSourcesMerged()
        {
            var from = new DateOnly(2026, 6, 1);
            var to = new DateOnly(2026, 6, 1);
            var calendarEvent = new Event(
                new DateTime(2026, 6, 1, 9, 0, 0),
                new DateTime(2026, 6, 1, 10, 0, 0),
                "Meeting", null, null, null, EventSource.Calendar);
            var taskEvent = new Event(
                new DateTime(2026, 6, 1, 11, 0, 0),
                new DateTime(2026, 6, 1, 13, 0, 0),
                "Task B", null, null, null, EventSource.Task);

            _source1Mock
                .Setup(s => s.GetEventsAsync(from, to, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Event> { calendarEvent });
            _source2Mock
                .Setup(s => s.GetEventsAsync(from, to, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Event> { taskEvent });

            var result = await _sut.GetEventsAsync(from, to, CancellationToken.None);

            Assert.That(result[from], Has.Count.EqualTo(2));
            Assert.That(result[from], Contains.Item(calendarEvent));
            Assert.That(result[from], Contains.Item(taskEvent));
        }

        [Test]
        public async Task GetEventsAsync_EventOutsideRange_NotIncluded()
        {
            var from = new DateOnly(2026, 6, 2);
            var to = new DateOnly(2026, 6, 3);
            var outsideEvent = new Event(
                new DateTime(2026, 6, 1, 10, 0, 0),
                new DateTime(2026, 6, 1, 11, 0, 0),
                "Old event", null, null, null, EventSource.Calendar);

            _source1Mock
                .Setup(s => s.GetEventsAsync(from, to, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Event> { outsideEvent });
            _source2Mock
                .Setup(s => s.GetEventsAsync(from, to, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Event>());

            var result = await _sut.GetEventsAsync(from, to, CancellationToken.None);

            Assert.That(result[from], Is.Empty);
            Assert.That(result[new DateOnly(2026, 6, 3)], Is.Empty);
        }

        [Test]
        public async Task GetEventsAsync_EventsWithinDay_SortedByStartTime()
        {
            var from = new DateOnly(2026, 6, 1);
            var to = new DateOnly(2026, 6, 1);
            var laterEvent = new Event(
                new DateTime(2026, 6, 1, 14, 0, 0),
                new DateTime(2026, 6, 1, 15, 0, 0),
                "Later", null, null, null, EventSource.Task);
            var earlierEvent = new Event(
                new DateTime(2026, 6, 1, 9, 0, 0),
                new DateTime(2026, 6, 1, 10, 0, 0),
                "Earlier", null, null, null, EventSource.Calendar);

            _source1Mock
                .Setup(s => s.GetEventsAsync(from, to, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Event> { laterEvent, earlierEvent });
            _source2Mock
                .Setup(s => s.GetEventsAsync(from, to, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Event>());

            var result = await _sut.GetEventsAsync(from, to, CancellationToken.None);

            Assert.That(result[from][0], Is.EqualTo(earlierEvent));
            Assert.That(result[from][1], Is.EqualTo(laterEvent));
        }
    }
}
