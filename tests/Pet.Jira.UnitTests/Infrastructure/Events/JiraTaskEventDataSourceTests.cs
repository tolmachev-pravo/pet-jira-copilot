using Atlassian.Jira;
using Moq;
using NUnit.Framework;
using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Events;
using Pet.Jira.Application.Storage;
using Pet.Jira.Application.Time;
using Pet.Jira.Domain.Models.Events;
using Pet.Jira.Domain.Models.Users;
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
    public class JiraTaskEventDataSourceTests
    {
        private Mock<IJiraService> _jiraServiceMock;
        private Mock<IJiraQueryFactory> _queryFactoryMock;
        private Mock<IIdentityService> _identityServiceMock;
        private Mock<IStorage<string, UserProfile>> _userProfileStorageMock;
        private Mock<ITimeProvider> _timeProviderMock;
        private IEventDataSource _sut;
        private CancellationToken _ct;

        [SetUp]
        public void Setup()
        {
            _jiraServiceMock = new Mock<IJiraService>();
            _queryFactoryMock = new Mock<IJiraQueryFactory>();
            _identityServiceMock = new Mock<IIdentityService>();
            _userProfileStorageMock = new Mock<IStorage<string, UserProfile>>();
            _timeProviderMock = new Mock<ITimeProvider>();
            _sut = new JiraTaskEventDataSource(
                _jiraServiceMock.Object,
                _queryFactoryMock.Object,
                _identityServiceMock.Object,
                _userProfileStorageMock.Object,
                _timeProviderMock.Object);
            _ct = CancellationToken.None;

            _identityServiceMock
                .Setup(x => x.GetCurrentUserAsync())
                .ReturnsAsync(new User { Username = "user1" });

            _userProfileStorageMock
                .Setup(x => x.GetValueAsync("user1", _ct))
                .ReturnsAsync(new UserProfile { Username = "user1", TimeZoneId = "UTC" });

            // Pass-through conversion so test times stay deterministic
            _timeProviderMock
                .Setup(x => x.ConvertToUserTimezone(It.IsAny<DateTime>(), It.IsAny<TimeZoneInfo>()))
                .Returns((DateTime dt, TimeZoneInfo _) => dt);

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
        public async Task GetEventsAsync_WhenIssueEntersAndLeavesInProgress_ReturnsInProgressInterval()
        {
            var issue = new IssueDto { Key = "PROJ-1", Summary = "Fix bug", Link = "http://jira/PROJ-1" };

            _jiraServiceMock
                .Setup(x => x.GetIssuesAsync(It.IsAny<IssueSearchOptions>(), _ct))
                .ReturnsAsync(new List<IssueDto> { issue });

            var t0 = new DateTime(2026, 6, 1, 9, 0, 0);
            var t1 = new DateTime(2026, 6, 1, 12, 0, 0);

            var changelogItems = new List<IssueChangeLogItemDto>
            {
                // -> In Progress (id 3)
                new IssueChangeLogItemDto
                {
                    ChangeLog = new IssueChangeLogDto { CreatedDate = t0, Issue = issue },
                    FromId = "1", ToId = "3", Author = "user1"
                },
                // In Progress -> Done
                new IssueChangeLogItemDto
                {
                    ChangeLog = new IssueChangeLogDto { CreatedDate = t1, Issue = issue },
                    FromId = "3", ToId = "4", Author = "user1"
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
            Assert.That(result[0].Source, Is.EqualTo(EventSource.Task));
            Assert.That(result[0].IssueKey, Is.EqualTo("PROJ-1"));
        }

        [Test]
        public async Task GetEventsAsync_WhenChangeLogItemAuthoredByAnotherUser_IsExcluded()
        {
            var issue = new IssueDto { Key = "PROJ-1", Summary = "Fix bug", Link = "http://jira/PROJ-1" };

            _jiraServiceMock
                .Setup(x => x.GetIssuesAsync(It.IsAny<IssueSearchOptions>(), _ct))
                .ReturnsAsync(new List<IssueDto> { issue });

            var t0 = new DateTime(2026, 6, 1, 9, 0, 0);
            var t1 = new DateTime(2026, 6, 1, 12, 0, 0);

            var changelogItems = new List<IssueChangeLogItemDto>
            {
                new IssueChangeLogItemDto
                {
                    ChangeLog = new IssueChangeLogDto { CreatedDate = t0, Issue = issue },
                    FromId = "1", ToId = "3", Author = "someone-else"
                },
                new IssueChangeLogItemDto
                {
                    ChangeLog = new IssueChangeLogDto { CreatedDate = t1, Issue = issue },
                    FromId = "3", ToId = "4", Author = "someone-else"
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

            Assert.That(result, Is.Empty);
        }
    }
}
