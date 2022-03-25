using System;
using System.Collections.Generic;

namespace Pet.Jira.Domain.Models.Worklogs
{
    public class DailyWorklogSummary
    {
        public DateTime Date { get; set; }
        public List<ActualWorklog> ActualWorklogs { get; set; }
        public List<EstimatedWorklog> EstimatedWorklogs { get; set; }
    }
}
