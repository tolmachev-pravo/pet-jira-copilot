using Atlassian.Jira;
using Pet.Jira.Infrastructure.Jira;
using Pet.Jira.Infrastructure.Jira.Query;
using System;

namespace Pet.Jira.Infrastructure.Events
{
    public class JiraTesterEventDataSource : JiraChangeLogEventDataSource
    {
        private const string InTestingStatusId = "10116";

        public JiraTesterEventDataSource(
            IJiraService jiraService,
            IJiraQueryFactory queryFactory)
            : base(jiraService, queryFactory) { }

        protected override string AssigneeField => "Tester";

        protected override Func<IssueChangeLogItem, bool> ChangeLogItemFilter =>
            item => item.FieldName == JiraConstants.Status.FieldName
                && (item.ToId == InTestingStatusId || item.FromId == InTestingStatusId);
    }
}
