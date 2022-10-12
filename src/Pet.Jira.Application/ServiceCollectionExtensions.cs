using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Pet.Jira.Application.Time;

namespace Pet.Jira.Application
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
        {
            services.AddMediatR(typeof(ServiceCollectionExtensions));
            services.AddSingleton<ITimeProvider, TimeProvider>();
            return services;
        }
    }
}
