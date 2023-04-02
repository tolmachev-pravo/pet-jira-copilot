using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Domain.Models.Issues;
using Pet.Jira.Domain.Models.Worklogs;
using Pet.Jira.UnitTests.Application.Mock;

namespace Pet.Jira.UnitTests.Application.Worklogs
{
    public class WorkingDayTests
    {
        private IIssue[] _issues;
        private DateTime _date;
        private WorkingDaySettings _defaultWorkingDaySettings;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _date = DateTime.Now.Date;
            _defaultWorkingDaySettings = new WorkingDaySettings(
                workingStartTime: TimeSpan.FromHours(9),
                workingEndTime: TimeSpan.FromHours(18),
                lunchTime: TimeSpan.FromHours(1));
            var i = 0;
            _issues = new[]
            {
                new MockIssue($"{i++}"),
                new MockIssue($"{i++}"),
                new MockIssue($"{i++}"),
                new MockIssue($"{i++}"),
            };
        }

        [Test]
        public void Refresh_WithActualAndEstimatedItems_Should_CalculateWorklogsSum()
        {
            // Arrange
            var worklogs = new List<WorkingDayWorklog>
            {
                new WorkingDayWorklog
                {
                    TimeSpent = TimeSpan.FromHours(3),
                    Type = WorklogType.Actual,
                    StartDate = _date.AddHours(9),
                    CompleteDate = _date.AddHours(12),
                    Issue = _issues[0]
                },
                new WorkingDayWorklog
                {
                    TimeSpent = TimeSpan.FromHours(4),
                    Type = WorklogType.Estimated,
                    StartDate = _date.AddHours(13),
                    CompleteDate = _date.AddHours(17),
                    Issue = _issues[1]
                }
            };
            var worklogCollectionDay = new WorkingDay(
                date: _date,
                settings: _defaultWorkingDaySettings,
                worklogs: worklogs);

            // Act
            worklogCollectionDay.Refresh();

            // Assert
            Assert.That(worklogCollectionDay.WorklogTimeSpent, Is.EqualTo(worklogCollectionDay.Settings.WorkingTime));
        }

        [Test]
        public void Refresh_WithActualItems_Should_CalculateTimeSpent()
        {
            // Arrange
            var worklogs = new List<WorkingDayWorklog>
            {
                new WorkingDayWorklog
                {
                    TimeSpent = TimeSpan.FromHours(3),
                    Type = WorklogType.Actual,
                    StartDate = _date.AddHours(9),
                    CompleteDate = _date.AddHours(12),
                    Issue = _issues[0]
                },
                new WorkingDayWorklog
                {
                    TimeSpent = TimeSpan.FromHours(2),
                    Type = WorklogType.Actual,
                    StartDate = _date.AddHours(13),
                    CompleteDate = _date.AddHours(15),
                    Issue = _issues[1]
                }
            };
            var worklogCollectionDay = new WorkingDay(
                date: _date,
                settings: _defaultWorkingDaySettings,
                worklogs: worklogs);

            // Act
            worklogCollectionDay.Refresh();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(worklogCollectionDay.ActualWorklogTimeSpent, Is.EqualTo(TimeSpan.FromHours(5)));
                Assert.That(worklogCollectionDay.WorklogTimeSpent, Is.EqualTo(TimeSpan.FromHours(5)));
            });
        }

        [Test]
        public void Refresh_Should_SetTimeSpentForEstimatedItems()
        {
            // Arrange
            var worklogs = new List<WorkingDayWorklog>
            {
                new WorkingDayWorklog
                {
                    TimeSpent = TimeSpan.FromHours(1),
                    Type = WorklogType.Actual,
                    Issue = _issues[0]
                },
                new WorkingDayWorklog
                {
                    TimeSpent = TimeSpan.FromHours(2),
                    Type = WorklogType.Estimated,
                    StartDate = _date.AddHours(9),
                    CompleteDate = _date.AddHours(11),
                    Issue = _issues[1]
                },
                new WorkingDayWorklog
                {
                    TimeSpent = TimeSpan.FromHours(0),
                    Type = WorklogType.Estimated,
                    StartDate = _date.AddHours(12),
                    CompleteDate = _date.AddHours(12),
                    Issue = _issues[2]
                },
            };
            var workingDay = new WorkingDay(
                date: _date,
                settings: _defaultWorkingDaySettings,
                worklogs: worklogs);

            // Act
            workingDay.Refresh();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(workingDay.ActualWorklogTimeSpent, Is.EqualTo(TimeSpan.FromHours(1)));
                Assert.That(workingDay.EstimatedWorklogTimeSpent, Is.EqualTo(TimeSpan.FromHours(7)));
                Assert.That(workingDay.WorklogTimeSpent, Is.EqualTo(TimeSpan.FromHours(8)));
                Assert.That(workingDay.Progress, Is.EqualTo(12));
            });
        }

        [Test]
        public void Refresh_Should_SetTimeSpentToZero_ForEmptyEstimatedItems()
        {
            // Arrange
            var worklogs = new List<WorkingDayWorklog>
            {
                new WorkingDayWorklog
                {
                    TimeSpent = TimeSpan.FromHours(1),
                    Type = WorklogType.Actual,
                    Issue = _issues[0]
                },
                new WorkingDayWorklog
                {
                    TimeSpent = TimeSpan.FromHours(0),
                    Type = WorklogType.Estimated,
                    Issue = _issues[1]
                },
            };
            var workingDay = new WorkingDay(
                date: _date,
                settings: _defaultWorkingDaySettings,
                worklogs: worklogs);

            // Act
            workingDay.Refresh();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(workingDay.ActualWorklogTimeSpent, Is.EqualTo(TimeSpan.FromHours(1)));
                Assert.That(workingDay.EstimatedWorklogTimeSpent, Is.EqualTo(TimeSpan.Zero));
                Assert.That(workingDay.WorklogTimeSpent, Is.EqualTo(TimeSpan.FromHours(1)));
                Assert.That(workingDay.Progress, Is.EqualTo(100));
            });
        }

        [Test]
        public void Refresh_Should_SetTimeSpentToZero_ForEstimatedWorklog_IfItHasAtLeastOneChild()
        {
            // Arrange
            var worklogs = new List<WorkingDayWorklog>
            {
                // 10:00 - 13:00 | issue_0 | estimated
                new WorkingDayWorklog
                {
                    TimeSpent = TimeSpan.FromHours(3),
                    Type = WorklogType.Estimated,
                    StartDate = _date.AddHours(10),
                    CompleteDate = _date.AddHours(13),
                    Issue = _issues[0]
                },
                // 13:00 - ..:.. | issue_0 | actual
                new WorkingDayWorklog
                {
                    TimeSpent = TimeSpan.FromHours(2),
                    Type = WorklogType.Actual,
                    StartDate = _date.AddHours(13),
                    Issue = _issues[0]
                },
            };
            var workingDay = new WorkingDay(
                date: _date,
                settings: _defaultWorkingDaySettings,
                worklogs: worklogs);

            // Act
            workingDay.Refresh();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(workingDay.ActualWorklogTimeSpent, Is.EqualTo(TimeSpan.FromHours(2)));
                Assert.That(workingDay.EstimatedWorklogTimeSpent, Is.EqualTo(TimeSpan.Zero));
                Assert.That(workingDay.WorklogTimeSpent, Is.EqualTo(TimeSpan.FromHours(2)));
                Assert.That(workingDay.Progress, Is.EqualTo(100));
            });
        }

        [Test]
        public void Refresh_Should_SetRemainingTimeSpent_ForEstimatedWorklogs_WithoutChildren()
        {
            // Arrange
            var worklogs = new List<WorkingDayWorklog>
            {
                // 10:00 - 13:00 | issue_0 | estimated
                new WorkingDayWorklog
                {
                    TimeSpent = TimeSpan.FromHours(3),
                    Type = WorklogType.Estimated,
                    StartDate = _date.AddHours(10),
                    CompleteDate = _date.AddHours(13),
                    Issue = _issues[0]
                },
                // 13:00 - ..:.. | issue_0 | actual
                new WorkingDayWorklog
                {
                    TimeSpent = TimeSpan.FromHours(2),
                    Type = WorklogType.Actual,
                    StartDate = _date.AddHours(13),
                    Issue = _issues[0]
                },
                // 13:00 - 17:00 | issue_1 | estimated
                new WorkingDayWorklog
                {
                    TimeSpent = TimeSpan.FromHours(4),
                    Type = WorklogType.Estimated,
                    StartDate = _date.AddHours(13),
                    CompleteDate = _date.AddHours(17),
                    Issue = _issues[1]
                },
            };
            var workingDay = new WorkingDay(
                date: _date,
                settings: _defaultWorkingDaySettings,
                worklogs: worklogs);

            // Act
            workingDay.Refresh();

            // Assert
            var issue0EstimatedWorklog = workingDay.EstimatedWorklogs.First(worklog => worklog.Issue.Key == _issues[0].Key);
            var issue1EstimatedWorklog = workingDay.EstimatedWorklogs.First(worklog => worklog.Issue.Key == _issues[1].Key);
            Assert.Multiple(() =>
            {
                Assert.That(workingDay.ActualWorklogTimeSpent, Is.EqualTo(TimeSpan.FromHours(2)));
                Assert.That(workingDay.EstimatedWorklogTimeSpent, Is.EqualTo(TimeSpan.FromHours(6)));
                Assert.That(workingDay.WorklogTimeSpent, Is.EqualTo(TimeSpan.FromHours(8)));
                Assert.That(workingDay.Progress, Is.EqualTo(25));
                Assert.That(issue0EstimatedWorklog.TimeSpent, Is.EqualTo(TimeSpan.Zero));
                Assert.That(issue0EstimatedWorklog.ChildrenTimeSpent, Is.EqualTo(TimeSpan.FromHours(2)));
                Assert.That(issue1EstimatedWorklog.TimeSpent, Is.EqualTo(TimeSpan.FromHours(6)));
                Assert.That(issue1EstimatedWorklog.ChildrenTimeSpent, Is.EqualTo(TimeSpan.Zero));
            });            
        }

        [Test]
        public void Refresh_Should_SetTimeSpentInProportions()
        {
            // Arrange
            var worklogs = new List<WorkingDayWorklog>
            {
                // 10:00 - 13:45 | issue_0 | estimated
                new WorkingDayWorklog
                {
                    TimeSpent =  new TimeSpan(3, 45, 0),
                    Type = WorklogType.Estimated,
                    StartDate = _date.AddHours(10),
                    CompleteDate = _date.AddHours(13).AddMinutes(45),
                    Issue = _issues[0]
                },
                // 14:00 - 16:15 | issue_1 | estimated
                new WorkingDayWorklog
                {
                    TimeSpent = new TimeSpan(2, 15, 0),
                    Type = WorklogType.Estimated,
                    StartDate = _date.AddHours(14),
                    CompleteDate = _date.AddHours(16).AddMinutes(15),
                    Issue = _issues[1]
                },
            };
            var workingDay = new WorkingDay(
                date: _date,
                settings: _defaultWorkingDaySettings,
                worklogs: worklogs);

            // Act
            workingDay.Refresh();

            // Assert
            var issue0EstimatedWorklog = workingDay.EstimatedWorklogs.First(worklog => worklog.Issue.Key == _issues[0].Key);
            var issue1EstimatedWorklog = workingDay.EstimatedWorklogs.First(worklog => worklog.Issue.Key == _issues[1].Key);
            Assert.Multiple(() =>
            {
                Assert.That(workingDay.ActualWorklogTimeSpent, Is.EqualTo(TimeSpan.FromHours(0)));
                Assert.That(workingDay.EstimatedWorklogTimeSpent, Is.EqualTo(TimeSpan.FromHours(8)));
                Assert.That(workingDay.WorklogTimeSpent, Is.EqualTo(TimeSpan.FromHours(8)));
                Assert.That(workingDay.Progress, Is.EqualTo(0));
                Assert.That(issue0EstimatedWorklog.TimeSpent, Is.EqualTo(TimeSpan.FromHours(5)));
                Assert.That(issue1EstimatedWorklog.TimeSpent, Is.EqualTo(TimeSpan.FromHours(3)));
            });
        }
    }
}
