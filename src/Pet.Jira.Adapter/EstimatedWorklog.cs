using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pet.Jira.Adapter
{
    /// <summary>
    /// 
    /// </summary>
    public class EstimatedWorklog
    {
        private TimeSpan? _timeSpent;
        private TimeSpan? _restTimeSpent;

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
            get { return _timeSpent ?? TimeSpan.FromSeconds(Math.Round((EndDate - StartDate).TotalSeconds)); }
            set { _timeSpent = value; }
        }


        public TimeSpan RawTimeSpent => TimeSpent;
        public TimeSpan EstimatedTimeSpent { get; set; }
        public TimeSpan ActualTimeSpent => new TimeSpan(ActualWorklogs.Sum(item => item.TimeSpent.Ticks));
        public TimeSpan RestTimeSpent
        {
            get { return _restTimeSpent ?? TimeSpan.FromSeconds(Math.Round((EstimatedTimeSpent - ActualTimeSpent).TotalSeconds)); }
            set { _restTimeSpent = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Issue Issue { get; set; }

        public ICollection<ActualWorklog> ActualWorklogs { get; set; }

        public EstimatedWorklog()
        {
            ActualWorklogs = new List<ActualWorklog>();
        }
    }
}
