using Microsoft.Extensions.DependencyInjection;
using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Issues;
using Pet.Jira.Application.Storage;
using Pet.Jira.Application.Worklogs;
using Pet.Jira.Domain.Models.Users;

namespace Pet.Jira.Infrastructure.Mock
{
    public static class MockServiceCollectionExtensions
    {
        public static IServiceCollection AddMockInfrastructureLayer(this IServiceCollection services)
        {
            services.AddTransient<IWorklogDataSource, MockWorklogDataSource>();
            services.AddTransient<IWorklogRepository, MockWorklogRepository>();
            services.AddTransient<IAuthenticationService, MockAuthenticationService>();
            services.AddTransient<IIssueDataSource, MockIssueDataSource>();
            services.AddTransient<IStorage<string, UserProfile>, MockUserProfileStorage>();
            return services;
        }
    }
}
