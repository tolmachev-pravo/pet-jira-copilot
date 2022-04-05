using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Worklogs;
using Pet.Jira.Infrastructure.Authentication;
using Pet.Jira.Infrastructure.Jira;
using Pet.Jira.Infrastructure.Jira.Query;
using Pet.Jira.Infrastructure.Worklogs;

namespace Pet.Jira.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration jiraConfigurationSection)
        {
            services.AddTransient<IJiraService, JiraService>();
            services.AddTransient<IWorklogDataSource, WorklogDataSource>();
            services.Configure<JiraConfiguration>(jiraConfigurationSection);
            services.AddSingleton<JiraLinkGenerator>();
            services.AddSingleton<WorklogFactory>();
            services.AddTransient<IWorklogRepository, WorklogRepository>();
            services.AddTransient<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<IJiraQueryFactory, JiraQueryFactory>();
            return services;
        }
    }
}
