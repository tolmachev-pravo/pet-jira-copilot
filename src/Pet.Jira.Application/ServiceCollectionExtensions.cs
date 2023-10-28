using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Pet.Jira.Application.Common.Behaviors;
using Pet.Jira.Application.Storage;
using Pet.Jira.Application.Time;
using Pet.Jira.Application.Users;
using Pet.Jira.Application.Worklogs;
using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Domain.Models.Users;
using System.Reflection;
using FluentValidation;

namespace Pet.Jira.Application
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
        {
            services.AddSingleton<ITimeProvider, TimeProvider>();
            services.AddSingleton<IMemoryCache<string, UserProfile>, UserProfileMemoryCache>();
            services.AddSingleton<IMemoryCache<string, UserTheme>, UserThemeMemoryCache>();
            services.AddSingleton<IMemoryCache<string, UserWorklogFilter>, UserWorklogFilterMemoryCache>();

			services.AddMediatR(Assembly.GetExecutingAssembly());
			services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
			services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

			services.AddAutoMapper(Assembly.GetExecutingAssembly());

			return services;
        }
    }
}
