using Pet.Jira.Application.Worklogs.Dto;
using System.Collections;

namespace Pet.Jira.UnitTests.Application.Worklogs
{
    public class WorkingDaySettingsTests
    {
        public class TestCases : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                yield return new object[] { TimeSpan.FromHours(9), TimeSpan.FromHours(18), TimeSpan.FromHours(1), TimeSpan.FromHours(8) };
                yield return new object[] { TimeSpan.FromHours(9), TimeSpan.FromHours(18), TimeSpan.FromHours(0), TimeSpan.FromHours(9) };
                yield return new object[] { TimeSpan.FromHours(17), TimeSpan.FromHours(18), TimeSpan.FromHours(1), TimeSpan.FromHours(0) };
                yield return new object[] { TimeSpan.FromHours(18), TimeSpan.FromHours(18), TimeSpan.FromHours(1), TimeSpan.FromHours(-1) };
            }
        }

        [TestCaseSource(typeof(TestCases))]
        public void WorkingTime_Should_BeCorrect(
            TimeSpan workingStartTime,
            TimeSpan workingEndTime,
            TimeSpan lunchTime,
            TimeSpan expectedWorkingTime)
        {
            // Arrange
            var settings = new WorkingDaySettings(
                workingStartTime: workingStartTime,
                workingEndTime: workingEndTime,
                lunchTime: lunchTime);

            // Act
            var workingTime = settings.WorkingTime;

            // Assert
            Assert.That(workingTime, Is.EqualTo(expectedWorkingTime));
        }
    }
}
