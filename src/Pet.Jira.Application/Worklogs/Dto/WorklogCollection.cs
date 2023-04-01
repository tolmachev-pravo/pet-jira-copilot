using System.Collections.Generic;

namespace Pet.Jira.Application.Worklogs.Dto
{
    public class WorklogCollection
    {
        /// <summary>
        /// List of days in worklog collection
        /// </summary>
        public IList<WorkingDay> Days { get; set; } = new List<WorkingDay>();
    }
}
