using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Pet.Jira.Domain.Models.Worklogs;

namespace Pet.Jira.Application.Worklogs
{
    public interface IWorklogDataSource
    {
        Task<IEnumerable<DailyWorklogSummary>> GetUserDayWorklogs(
            DateTime fromDate,
            DateTime toDate,
            int issueCount);
    }
}
