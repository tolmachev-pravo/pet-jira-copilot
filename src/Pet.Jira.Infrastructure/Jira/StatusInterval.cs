using Pet.Jira.Infrastructure.Jira.Dto;
using System;

namespace Pet.Jira.Infrastructure.Jira
{
    /// <summary>
    /// A worklog-agnostic time interval during which an issue stayed in a given
    /// status, derived from ordered status-change changelog items. Both the
    /// worklog and event pipelines map from this primitive.
    /// </summary>
    public readonly record struct StatusInterval(
        DateTime Start,
        DateTime End,
        string Author,
        IssueDto Issue)
    {
        /// <summary>
        /// True when the interval overlaps the [from, to] range.
        /// </summary>
        public bool OverlapsWith(DateTime from, DateTime to)
            => Start < to && End > from;
    }
}
