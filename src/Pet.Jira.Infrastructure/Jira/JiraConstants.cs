namespace Pet.Jira.Infrastructure.Jira
{
    public static class JiraConstants
    {
        public const string DateTimeFormat = "yyyy/MM/dd";

        public static class Status
        {
            public const string InProgress = "3";
            public const string FieldName = "status";
        }

        public static class Date
        {
            public const int StartWorkingTime = 10;
            public const int EndWorkingTime = 19;
        }
    }
}
