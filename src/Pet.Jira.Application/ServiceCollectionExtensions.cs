using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Pet.Jira.Application.Storage;
using Pet.Jira.Application.Time;
using Pet.Jira.Application.Users;
using Pet.Jira.Application.Worklogs;
using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Domain.Models.Users;

namespace Pet.Jira.Application
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
        {
            services.AddMediatR(typeof(ServiceCollectionExtensions));
            services.AddSingleton<ITimeProvider, TimeProvider>();
            services.AddSingleton<IMemoryCache<string, UserProfile>, UserProfileMemoryCache>();
            services.AddSingleton<IMemoryCache<string, UserTheme>, UserThemeMemoryCache>();
            services.AddSingleton<IMemoryCache<string, UserWorklogFilter>, UserWorklogFilterMemoryCache>();
            return services;
        }
    }
}
