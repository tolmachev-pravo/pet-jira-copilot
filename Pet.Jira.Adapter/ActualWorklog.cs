using System;

namespace Pet.Jira.Adapter
{
    /// <summary>
    /// 
    /// </summary>
    public class ActualWorklog
    {
        /// <summary>
        /// 
        /// </summary>
        public Issue Issue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan TimeSpent { get; set; }
    }
}
