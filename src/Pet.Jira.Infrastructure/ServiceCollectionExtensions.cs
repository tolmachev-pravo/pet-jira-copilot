using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Issues;
using Pet.Jira.Application.Storage;
using Pet.Jira.Application.Users;
using Pet.Jira.Application.Worklogs;
using Pet.Jira.Domain.Models.Users;
using Pet.Jira.Infrastructure.Authentication;
using Pet.Jira.Infrastructure.Jira;
using Pet.Jira.Infrastructure.Jira.Query;
using Pet.Jira.Infrastructure.Storage;
using Pet.Jira.Infrastructure.Users;
using Pet.Jira.Infrastructure.Worklogs;

namespace Pet.Jira.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration jiraConfigurationSection)
        {
            services.AddTransient<IJiraService, JiraService>();
            services.AddTransient<IWorklogDataSource, JiraWorklogDataSource>();
            services.Configure<JiraConfiguration>(jiraConfigurationSection);
            services.AddSingleton<IJiraLinkGenerator, JiraLinkGenerator>();
            services.AddTransient<IWorklogRepository, WorklogRepository>();
            services.AddTransient<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<IJiraQueryFactory, JiraQueryFactory>();
            services.AddTransient<IIssueDataSource, JiraIssueDataSource>();
            services.AddTransient<IUserDataSource, JiraUserDataSource>();
            services.AddSingleton<IUserStorage, JiraUserStorage>();

            services.AddTransient<IStorage<string, UserProfile>, UserProfileStorage>();
            services.AddTransient<ILocalStorage<UserProfile>, UserProfileLocalStorage>();
            services.AddSingleton<ILoginMemoryCache, LoginMemoryCache>();
            return services;
        }
    }
}
