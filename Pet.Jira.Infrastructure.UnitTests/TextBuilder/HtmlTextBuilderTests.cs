using Pet.Jira.Application.TextBuilder;
using Pet.Jira.Infrastructure.TextBuilder;

namespace Pet.Jira.Infrastructure.UnitTests.TextBuilder
{
    [TestFixture]
    public class HtmlTextBuilderTests
    {
        private HtmlTextBuilder _htmlTextBuilder;

        [SetUp]
        public void SetUp()
        {
            _htmlTextBuilder = new HtmlTextBuilder();
        }

        [TestCase("https://localhost.ru", "site url", $"<a href=\"https://localhost.ru\">site url</a> ")]
        [TestCase(null, "site url", $"<a href=\"\">site url</a> ")]
        [TestCase("https://localhost.ru", null, $"<a href=\"https://localhost.ru\"></a> ")]
        [TestCase(null, null, $"<a href=\"\"></a> ")]
        public void AddLink_Should_BeCorrect(string href, string value, string expected)
        {
            // Arrange
            // Act
            _htmlTextBuilder.AddLink(href, value);

            // Assert
            var result = _htmlTextBuilder.Build();
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase("<b>text</b> ", "text", TextOption.Bold)]
        [TestCase("<b><b>text</b></b> ", "text", TextOption.Bold, TextOption.Bold)]
        [TestCase("text ", "text")]
        [TestCase(" ", "")]
        [TestCase(" ", null)]
        public void AddText_Should_BeCorrect(string expected, string text, params TextOption[] options)
        {
            // Arrange
            // Act
            _htmlTextBuilder.AddText(text, options);

            // Assert
            var result = _htmlTextBuilder.Build();
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase("<b>", TextOption.Bold)]
        public void BeginOption_Should_BeCorrect(string expected, TextOption option)
        {
            // Arrange
            // Act
            _htmlTextBuilder.BeginOption(option);

            // Assert
            var result = _htmlTextBuilder.Build();
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase("</b>", TextOption.Bold)]
        public void EndOption_Should_BeCorrect(string expected, TextOption option)
        {
            // Arrange
            // Act
            _htmlTextBuilder.EndOption(option);

            // Assert
            var result = _htmlTextBuilder.Build();
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
