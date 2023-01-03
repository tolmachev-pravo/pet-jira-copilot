using Moq;
using Pet.Jira.Application.Time;
using Pet.Jira.Domain.Models.Worklogs;
using Pet.Jira.Infrastructure.Jira;
using Pet.Jira.Infrastructure.Jira.Dto;

namespace Pet.Jira.Infrastructure.UnitTests.Jira
{
    [TestFixture]
    public class JiraExtensionsTests
    {
        private ITimeProvider _timeProvider;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var timeProviderMock = new Mock<ITimeProvider>();
            timeProviderMock
                .Setup(mock => mock.ConvertToUserTimezone(It.IsAny<DateTime>(), It.IsAny<TimeZoneInfo>()))
                .Returns((DateTime dateTime, TimeZoneInfo userTimeZone) => dateTime);
            _timeProvider = timeProviderMock.Object;
        }

        [Test]
        public void ConvertTo_Should_BeCorrect()
        {
            // Arrange
            var comment = new IssueCommentDto
            {
                CreatedDate = new DateTime(2022, 01, 03, 13, 22, 10),
                Author = "Author #1",
                Issue = new Mock<IssueDto>().Object
            };
            List<IssueCommentDto> comments = new() { comment };

            // Act
            var worklogs = comments.ConvertTo<RawIssueWorklog>(_timeProvider, It.IsAny<TimeZoneInfo>(), WorklogSource.Comment, TimeSpan.FromMinutes(10));

            // Assert
            var worklog = worklogs.Single();
            Assert.That(worklog.Author, Is.EqualTo(comment.Author));
            Assert.That(worklog.TimeSpent, Is.EqualTo(TimeSpan.FromMinutes(10)));
            Assert.That(worklog.CompleteDate, Is.EqualTo(comment.CreatedDate));
            Assert.That(worklog.StartDate, Is.EqualTo(comment.CreatedDate.AddMinutes(-10)));
        }
    }
}
