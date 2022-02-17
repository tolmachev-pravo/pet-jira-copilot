using System;
using System.Collections.Generic;
using System.Text;

namespace Pet.Jira.Adapter
{
    /// <summary>
    /// 
    /// </summary>
    public class EstimatedWorklog
    {
        private TimeSpan? _timeSpent;

        /// <summary>
        /// 
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan TimeSpent
        {
            get { return _timeSpent ?? (EndDate - StartDate); }
            set { _timeSpent = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Issue Issue { get; set; }
    }
}
