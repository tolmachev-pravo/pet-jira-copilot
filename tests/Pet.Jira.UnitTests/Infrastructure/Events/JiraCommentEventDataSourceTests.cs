using Atlassian.Jira;
using Moq;
using NUnit.Framework;
using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Events;
using Pet.Jira.Application.Storage;
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
    public class JiraCommentEventDataSourceTests
    {
        private Mock<IJiraService> _jiraServiceMock;
        private Mock<IJiraQueryFactory> _queryFactoryMock;
        private Mock<IIdentityService> _identityServiceMock;
        private Mock<IStorage<string, UserProfile>> _userProfileStorageMock;
        private IEventDataSource _sut;
        private CancellationToken _ct;

        [SetUp]
        public void Setup()
        {
            _jiraServiceMock = new Mock<IJiraService>();
            _queryFactoryMock = new Mock<IJiraQueryFactory>();
            _identityServiceMock = new Mock<IIdentityService>();
            _userProfileStorageMock = new Mock<IStorage<string, UserProfile>>();
            _sut = new JiraCommentEventDataSource(
                _jiraServiceMock.Object,
                _queryFactoryMock.Object,
                _identityServiceMock.Object,
                _userProfileStorageMock.Object);
            _ct = CancellationToken.None;

            _identityServiceMock
                .Setup(x => x.GetCurrentUserAsync())
                .ReturnsAsync(new Pet.Jira.Domain.Models.Users.User { Username = "user1" });

            _userProfileStorageMock
                .Setup(x => x.GetValueAsync("user1", _ct))
                .ReturnsAsync(new UserProfile { Username = "user1", TimeZoneId = "UTC" });

            _queryFactoryMock.Setup(x => x.Create()).Returns(new JiraQuery());
        }

        [Test]
        public async Task GetEventsAsync_WhenNoComments_ReturnsEmptyList()
        {
            _jiraServiceMock
                .Setup(x => x.GetIssuesAsync(It.IsAny<IssueSearchOptions>(), _ct))
                .ReturnsAsync(new List<IssueDto>());

            _jiraServiceMock
                .Setup(x => x.GetIssueCommentsAsync(
                    It.IsAny<IEnumerable<IssueDto>>(),
                    It.IsAny<Func<Comment, bool>>(),
                    _ct))
                .ReturnsAsync(new List<IssueCommentDto>());

            var result = await _sut.GetEventsAsync(
                new DateOnly(2026, 6, 1),
                new DateOnly(2026, 6, 1),
                _ct);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetEventsAsync_WhenCommentsExist_ReturnsMappedEventsWithStartEqualsEnd()
        {
            var issue = new IssueDto { Key = "PROJ-1", Summary = "Some task", Link = "http://jira/PROJ-1" };
            var commentTime = new DateTime(2026, 6, 1, 14, 0, 0);
            var comment = new IssueCommentDto
            {
                CreatedDate = commentTime,
                Issue = issue,
                Author = "user1",
                Body = "Done."
            };

            _jiraServiceMock
                .Setup(x => x.GetIssuesAsync(It.IsAny<IssueSearchOptions>(), _ct))
                .ReturnsAsync(new List<IssueDto> { issue });

            _jiraServiceMock
                .Setup(x => x.GetIssueCommentsAsync(
                    It.IsAny<IEnumerable<IssueDto>>(),
                    It.IsAny<Func<Comment, bool>>(),
                    _ct))
                .ReturnsAsync(new List<IssueCommentDto> { comment });

            var result = await _sut.GetEventsAsync(
                new DateOnly(2026, 6, 1),
                new DateOnly(2026, 6, 1),
                _ct);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Start, Is.EqualTo(commentTime));
            Assert.That(result[0].End, Is.EqualTo(commentTime));
            Assert.That(result[0].Title, Is.EqualTo("PROJ-1. Some task"));
            Assert.That(result[0].Description, Is.EqualTo("Done."));
            Assert.That(result[0].Source, Is.EqualTo(EventSource.Comment));
            Assert.That(result[0].Issue, Is.Not.Null);
        }
    }
}
