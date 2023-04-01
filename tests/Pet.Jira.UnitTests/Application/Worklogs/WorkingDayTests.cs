using Pet.Jira.Application.Worklogs.Dto;

namespace Pet.Jira.UnitTests.Application.Worklogs
{
    public class WorkingDayTests
    {
        [Test]
        public void Refresh_WithActualAndEstimatedItems_ShouldCalculateWorklogsSum()
        {
            // Arrange
            var date = DateTime.Now.Date;
            var settings = new WorkingDaySettings(
                workingStartTime: TimeSpan.FromHours(9),
                workingEndTime: TimeSpan.FromHours(18),
                lunchTime: TimeSpan.FromHours(1));
            var worklogs = new List<WorklogCollectionItem>
            {
                new WorklogCollectionItem
                {
                    TimeSpent = TimeSpan.FromHours(3),
                    Type = WorklogCollectionItemType.Actual,
                    StartDate = DateTime.Now.Date.AddHours(9),
                    CompleteDate = DateTime.Now.Date.AddHours(12)
                },
                new WorklogCollectionItem
                {
                    TimeSpent = TimeSpan.FromHours(4),
                    Type = WorklogCollectionItemType.Estimated,
                    StartDate = DateTime.Now.Date.AddHours(13),
                    CompleteDate = DateTime.Now.Date.AddHours(17)
                }
            };
            var worklogCollectionDay = new WorkingDay(
                date: date,
                settings: settings,
                worklogs: worklogs);

            // Act
            worklogCollectionDay.Refresh();

            // Assert
            Assert.That(worklogCollectionDay.CalculatedWorklogsSum, Is.EqualTo(worklogCollectionDay.Settings.WorkingTime));
        }

        [Test]
        public void Refresh_WithAutoAndManualActualItems_ShouldCalculateAutoAndManualTimeSpent()
        {
            // Arrange
            var date = DateTime.Now.Date;
            var settings = new WorkingDaySettings(
                workingStartTime: TimeSpan.FromHours(9),
                workingEndTime: TimeSpan.FromHours(18),
                lunchTime: TimeSpan.FromHours(1));
            var worklogs = new List<WorklogCollectionItem>
            {
                new WorklogCollectionItem
                {
                    TimeSpent = TimeSpan.FromHours(3),
                    Type = WorklogCollectionItemType.Actual,
                    StartDate = DateTime.Now.Date.AddHours(9),
                    CompleteDate = DateTime.Now.Date.AddHours(12)
                },
                new WorklogCollectionItem
                {
                    TimeSpent = TimeSpan.FromHours(2),
                    Type = WorklogCollectionItemType.Actual,
                    StartDate = DateTime.Now.Date.AddHours(13),
                    CompleteDate = DateTime.Now.Date.AddHours(15)
                }
            };
            var worklogCollectionDay = new WorkingDay(
                date: date,
                settings: settings,
                worklogs: worklogs);

            // Act
            worklogCollectionDay.Refresh();

            // Assert
            Assert.That(worklogCollectionDay.ActualWorklogsSum, Is.EqualTo(TimeSpan.FromHours(5)));
            Assert.That(worklogCollectionDay.CalculatedWorklogsSum, Is.EqualTo(TimeSpan.FromHours(5)));
        }

        [Test]
        public void Refresh_Should_SetTimeSpentForEstimatedItems()
        {
            // Arrange
            var date = DateTime.Now.Date;
            var settings = new WorkingDaySettings(
                workingStartTime: TimeSpan.FromHours(9),
                workingEndTime: TimeSpan.FromHours(18),
                lunchTime: TimeSpan.FromHours(1));
            var worklogs = new List<WorklogCollectionItem>
            {
                new WorklogCollectionItem
                {
                    TimeSpent = TimeSpan.FromHours(1),
                    Type = WorklogCollectionItemType.Actual,
                },
                new WorklogCollectionItem
                {
                    TimeSpent = TimeSpan.FromHours(2),
                    Type = WorklogCollectionItemType.Estimated,
                    StartDate = DateTime.Now.Date.AddHours(9),
                    CompleteDate = DateTime.Now.Date.AddHours(11)
                },
                new WorklogCollectionItem
                {
                    TimeSpent = TimeSpan.FromHours(0),
                    Type = WorklogCollectionItemType.Estimated,
                    StartDate = DateTime.Now.Date.AddHours(12),
                    CompleteDate = DateTime.Now.Date.AddHours(12)
                },
            };
            var workingDay = new WorkingDay(
                date: date,
                settings: settings,
                worklogs: worklogs);

            // Act
            workingDay.Refresh();

            // Assert
            Assert.That(workingDay.ActualWorklogsSum, Is.EqualTo(TimeSpan.FromHours(1)));
            Assert.That(workingDay.EstimatedWorklogsSum, Is.EqualTo(TimeSpan.FromHours(7)));
            Assert.That(workingDay.CalculatedWorklogsSum, Is.EqualTo(TimeSpan.FromHours(8)));
            Assert.That(workingDay.Progress, Is.EqualTo(12));
        }

        [Test]
        public void Refresh_Should_SetTimeSpentToZero_ForEmptyEstimatedItems()
        {
            // Arrange
            var date = DateTime.Now.Date;
            var settings = new WorkingDaySettings(
                workingStartTime: TimeSpan.FromHours(9),
                workingEndTime: TimeSpan.FromHours(18),
                lunchTime: TimeSpan.FromHours(1));
            var worklogs = new List<WorklogCollectionItem>
            {
                new WorklogCollectionItem
                {
                    TimeSpent = TimeSpan.FromHours(1),
                    Type = WorklogCollectionItemType.Actual,
                },
                new WorklogCollectionItem
                {
                    TimeSpent = TimeSpan.FromHours(0),
                    Type = WorklogCollectionItemType.Estimated
                },
            };
            var workingDay = new WorkingDay(
                date: date,
                settings: settings,
                worklogs: worklogs);

            // Act
            workingDay.Refresh();

            // Assert
            Assert.That(workingDay.ActualWorklogsSum, Is.EqualTo(TimeSpan.FromHours(1)));
            Assert.That(workingDay.EstimatedWorklogsSum, Is.EqualTo(TimeSpan.Zero));
            Assert.That(workingDay.CalculatedWorklogsSum, Is.EqualTo(TimeSpan.FromHours(1)));
            Assert.That(workingDay.Progress, Is.EqualTo(100));
        }
    }
}
