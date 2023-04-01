using System;

namespace Pet.Jira.Domain.Models.Worklogs
{
    public interface IHasTimeSpent
    {
        public TimeSpan TimeSpent { get; }
    }
}
