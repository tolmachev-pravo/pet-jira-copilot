using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Domain.Models.Worklogs;
using System;
using System.Collections.Generic;

namespace Pet.Jira.Application.Worklogs
{
    public interface IWorklogDistributor
    {
        public IEnumerable<WorkingDay> DistributeByDays(
            IEnumerable<IWorklog> worklogs,
            DateTime firstDate,
            DateTime lastDate,
            WorkingDaySettings daySettings);
    }
}
