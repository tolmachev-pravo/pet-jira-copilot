using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Pet.Jira.Adapter;

namespace Pet.Jira.App
{
    class Program
    {
        private static string HtmlToPlainText(string html)
        {
            const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";//matches one or more (white space or line breaks) between '>' and '<'
            const string stripFormatting = @"<[^>]*(>|$)";//match any character between '<' and '>', even when end tag is missing
            const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";//matches: <br>,<br/>,<br />,<BR>,<BR/>,<BR />
            var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
            var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
            var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);

            var text = html;
            //Decode html specific characters
            text = System.Net.WebUtility.HtmlDecode(text);
            //Remove tag whitespace/line breaks
            text = tagWhiteSpaceRegex.Replace(text, "><");
            //Replace <br /> with line breaks
            text = lineBreakRegex.Replace(text, Environment.NewLine);
            //Strip formatting
            text = stripFormattingRegex.Replace(text, string.Empty);

            return text;
        }

        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services, args);
            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                await serviceProvider.GetService<IStartup>().Run();
            }



            using var client = new HttpClient();
            var userName = "xxxxxx ";
            var passwd = "xxxxxx ";
            var authToken = Encoding.ASCII.GetBytes($"{userName}:{passwd}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(authToken));
            var result = await client.GetAsync("https://jira.parcsis.org/activity?maxResults=300&streams=user+IS+xxxxxx &os_authType=basic&title=undefined");
            var content = await result.Content.ReadAsStringAsync();
            XmlSerializer serializer = new XmlSerializer(typeof(feed));
            feed feed = null;
            using (TextReader reader = new StringReader(content))
            {
                feed = (feed)serializer.Deserialize(reader);
            }

            var list = new List<MyFeedEntry>();

            foreach (var entry in feed.entry)
            {
                var status = entry.title.Value switch
                {
                    string Value when Value.Contains("started progress on") => Status.InProgress,
                    string Value when Value.Contains("In Review") => Status.InReview,
                    string Value when Value.Contains("reopened") => Status.Reopened,
                    string Value when Value.Contains("Waiting") => Status.Waiting,
                    _ => Status.Unknown
                };

                list.Add(new MyFeedEntry()
                {
                    Name = entry.@object?.title?.Value,
                    Description = entry.@object?.summary?.Value,
                    At = entry.published.ToLocalTime(),
                    Status = status,
                    Link = entry.@object?.link?.href,
                    Title = HtmlToPlainText(entry.title?.Value)
                });
            }

            var startDate = DateTime.Now.Date;
            Dictionary<DateTime, List<MyFeedEntry>> dictionary = new Dictionary<DateTime, List<MyFeedEntry>>();
            while (startDate > new DateTime(2022, 01, 01))
            {
                dictionary.Add(startDate, list.Where(item => item.At.Date == startDate).ToList());
                startDate = startDate.AddDays(-1);
            }

            foreach (var dictionaryitem in dictionary)
            {
                Console.WriteLine($"{dictionaryitem.Key:dd/MM/yyyy} - {CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat.GetDayName(dictionaryitem.Key.DayOfWeek)}");
                foreach (var entry in dictionaryitem.Value)
                {
                    Console.WriteLine(entry);
                }
            }
        }

        private static void ConfigureServices(IServiceCollection services, string[] args)
        {
            var configuration = SetupConfiguration(args);
            services
                .AddLogging(config => config
                    .ClearProviders()
                    .AddConsole()
                    .SetMinimumLevel(LogLevel.Trace)
                )
                .AddSingleton<IConfiguration>(configuration)
                .AddSingleton<IStartup, Startup>()
                .AddSingleton<IJiraConfiguration, JiraConfiguration>()
                .AddTransient<JiraService>();
        }

        private static IConfigurationRoot SetupConfiguration(string[] args)
        {
            return new ConfigurationBuilder()
                .AddCommandLine(args)
                .AddJsonFile($"appsettings.json")
                .Build();
        }
    }

    public enum Status
    {
        Unknown,
        Reopened,
        Waiting,
        InProgress,
        InReview
    }

    public class MyFeedEntry
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime At { get; set; }
        public Status Status { get; set; }
        public string Link { get; set; }
        public string Title { get; set; }

        public override string ToString()
        {
            return $"{At:dd/MM/yyyy HH:mm:ss}\t{Name ?? "CASEM-xxxxx"}\t{Status.ToString().ToFixedString(10)}\t{Link}\t{Description}";
//{Title}";
        }
    }
}
