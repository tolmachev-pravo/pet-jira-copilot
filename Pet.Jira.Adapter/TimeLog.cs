using System;

namespace Pet.Jira.Adapter
{
    public class TimeLog
    {
        public DateTime From { get; set; }

        public DateTime To { get; set; }

        public string IssueName { get; set; }

        public string IssueSummary { get; set; }

        public string IssueLink { get; set; }

        private TimeSpan? _diff;

        public TimeSpan Diff
        {
            get { return _diff ?? (To - From); }
            set { _diff = value; }
        }
    }
}
