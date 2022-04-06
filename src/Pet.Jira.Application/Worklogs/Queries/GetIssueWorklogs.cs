using System;

namespace Pet.Jira.Application.Worklogs.Queries
{
    public class GetIssueWorklogs
    {
        public class Query
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }
    }
}
