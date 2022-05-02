using System.Collections.Generic;

namespace Pet.Jira.Application.Worklogs.Dto
{
    public class WorklogCollection
    {
        public IList<WorklogCollectionDay> Days { get; set; } = new List<WorklogCollectionDay>();
    }
}
