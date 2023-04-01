using System;
using System.ComponentModel.DataAnnotations;

namespace Pet.Jira.Application.Worklogs.Dto
{
    /// <summary>
    /// Settings of working day
    /// </summary>
    public class WorkingDaySettings
    {
        /// <summary>
        /// Working start time
        /// </summary>
        [Required]
        public TimeSpan WorkingStartTime { get; set; }

        /// <summary>
        /// Working end time
        /// </summary>
        [Required]
        public TimeSpan WorkingEndTime { get; set; }

        /// <summary>
        /// Time for lunch
        /// </summary>
        [Required]
        public TimeSpan LunchTime { get; set; } = TimeSpan.FromHours(1);

        /// <summary>
        /// Working time
        /// </summary>
        public TimeSpan WorkingTime => WorkingEndTime - WorkingStartTime - LunchTime;

        public WorkingDaySettings(
            TimeSpan workingStartTime,
            TimeSpan workingEndTime,
            TimeSpan lunchTime)
        {
            WorkingStartTime = workingStartTime;
            WorkingEndTime = workingEndTime;
            LunchTime = lunchTime;
        }
    }
}
