using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Pet.Jira.Infrastructure.Jira.Query
{
    public class JiraQuery
    {
        private readonly List<JiraQueryCondition> conditions;
        private readonly List<JiraQueryOrder> orders;

        public JiraQuery()
        {
            conditions = new List<JiraQueryCondition>();
            orders = new List<JiraQueryOrder>();
        }

        public JiraQuery Where(string left, JiraQueryComparisonType comparisonType, object right)
        {
            conditions.Add(new JiraQueryCondition
            {
                Left = left,
                ComparisonType = comparisonType,
                Right = GetStringValue(right)
            });
            return this;
        }

        public JiraQuery OrderBy(string parameter, JiraQueryOrderType orderType = JiraQueryOrderType.Asc)
        {
            orders.Add(new JiraQueryOrder
            {
                Parameter = parameter,
                OrderType = orderType
            });
            return this;
        }

        private string GetStringValue(object value)
        {
            var stringValue = value.ToString();
            switch (value)
            {
                case JiraQueryMacros macrosValue:
                    return JiraQueryConstants.Macroses[macrosValue];
                case DateTime dateValue:
                    stringValue = dateValue.ToString(JiraQueryConstants.Date.DefaultFormat, CultureInfo.InvariantCulture);
                    break;
            }
            return $"'{stringValue}'";
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            if (conditions.Any())
            {
                stringBuilder.Append(string.Join(" AND ", conditions));
                stringBuilder.Append(" ");
            }
            if (orders.Any())
            {
                stringBuilder.Append("ORDER BY ");
                stringBuilder.Append(string.Join(", ", orders));
                stringBuilder.Append(" ");
            }

            return stringBuilder.ToString();
        }
    }
}
