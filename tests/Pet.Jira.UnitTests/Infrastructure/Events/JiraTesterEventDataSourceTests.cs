using Atlassian.Jira;
using Moq;
using NUnit.Framework;
using Pet.Jira.Application.Events;
using Pet.Jira.Infrastructure.Events;
using Pet.Jira.Infrastructure.Jira;
using Pet.Jira.Infrastructure.Jira.Dto;
using Pet.Jira.Infrastructure.Jira.Query;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.UnitTests.Infrastructure.Events
{
    [TestFixture]
    public class JiraTesterEventDataSourceTests
    {
        private Mock<IJiraService> _jiraServiceMock;
        private Mock<IJiraQueryFactory> _queryFactoryMock;
        private IEventDataSource _sut;
        private CancellationToken _ct;

        [SetUp]
        public void Setup()
        {
            _jiraServiceMock = new Mock<IJiraService>();
            _queryFactoryMock = new Mock<IJiraQueryFactory>();
            _sut = new JiraTesterEventDataSource(
                _jiraServiceMock.Object,
                _queryFactoryMock.Object);
            _ct = CancellationToken.None;

            _queryFactoryMock.Setup(x => x.Create()).Returns(new JiraQuery());
        }

        [Test]
        public async Task GetEventsAsync_WhenNoIssues_ReturnsEmptyList()
        {
            _jiraServiceMock
                .Setup(x => x.GetIssuesAsync(It.IsAny<IssueSearchOptions>(), _ct))
                .ReturnsAsync(new List<IssueDto>());

            _jiraServiceMock
                .Setup(x => x.GetIssueChangeLogItemsAsync(
                    It.IsAny<IEnumerable<IssueDto>>(),
                    It.IsAny<Func<IssueChangeLog, bool>>(),
                    It.IsAny<Func<IssueChangeLogItem, bool>>(),
                    _ct))
                .ReturnsAsync(new List<IssueChangeLogItemDto>());

            var result = await _sut.GetEventsAsync(
                new DateOnly(2026, 6, 1),
                new DateOnly(2026, 6, 1),
                _ct);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetEventsAsync_WhenIssueHasTesterChangelogItems_ReturnsEventIntervals()
        {
            var issue = new IssueDto { Key = "PROJ-2", Summary = "Test the feature", Link = "http://jira/PROJ-2" };

            _jiraServiceMock
                .Setup(x => x.GetIssuesAsync(It.IsAny<IssueSearchOptions>(), _ct))
                .ReturnsAsync(new List<IssueDto> { issue });

            var t0 = new DateTime(2026, 6, 1, 10, 0, 0);
            var t1 = new DateTime(2026, 6, 1, 14, 0, 0);

            var changelogItems = new List<IssueChangeLogItemDto>
            {
                new IssueChangeLogItemDto
                {
                    ChangeLog = new IssueChangeLogDto { CreatedDate = t0, Issue = issue },
                    FromId = "3", ToId = "10116"
                },
                new IssueChangeLogItemDto
                {
                    ChangeLog = new IssueChangeLogDto { CreatedDate = t1, Issue = issue },
                    FromId = "10116", ToId = "4"
                }
            };

            _jiraServiceMock
                .Setup(x => x.GetIssueChangeLogItemsAsync(
                    It.IsAny<IEnumerable<IssueDto>>(),
                    It.IsAny<Func<IssueChangeLog, bool>>(),
                    It.IsAny<Func<IssueChangeLogItem, bool>>(),
                    _ct))
                .ReturnsAsync(changelogItems);

            var result = await _sut.GetEventsAsync(
                new DateOnly(2026, 6, 1),
                new DateOnly(2026, 6, 1),
                _ct);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Start, Is.EqualTo(t0));
            Assert.That(result[0].End, Is.EqualTo(t1));
            Assert.That(result[0].Source, Is.EqualTo(Pet.Jira.Domain.Models.Events.EventSource.Task));
            Assert.That(result[0].Issue!.Key, Is.EqualTo("PROJ-2"));
        }
    }
}
