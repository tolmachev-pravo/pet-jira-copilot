using Atlassian.Jira;
using Pet.Jira.Infrastructure.Jira;
using Pet.Jira.Infrastructure.Jira.Query;
using System;

namespace Pet.Jira.Infrastructure.Events
{
    public class JiraTaskEventDataSource : JiraChangeLogEventDataSource
    {
        public JiraTaskEventDataSource(
            IJiraService jiraService,
            IJiraQueryFactory queryFactory)
            : base(jiraService, queryFactory) { }

        protected override string AssigneeField => "assignee";

        protected override Func<IssueChangeLogItem, bool> ChangeLogItemFilter =>
            item => item.FieldName == JiraConstants.Status.FieldName;
    }
}
