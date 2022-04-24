using System;

namespace Pet.Jira.Application.Worklogs.Queries
{
    public class GetRawIssueWorklogs
    {
        public class Query
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string IssueStatusId { get; set; }
        }
    }
}