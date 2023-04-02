using Pet.Jira.Domain.Models.Issues;

namespace Pet.Jira.UnitTests.Application.Mock
{
    internal class MockIssue : IIssue
    {
        public MockIssue(string key)
        {
            Key = key;
        }

        public string Key { get; set; }
        public string Summary { get; set; }
        public string Link { get; set; }
        public string Identifier { get; set; }
    }
}
