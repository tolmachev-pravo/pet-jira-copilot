using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace Pet.Jira.Application
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
        {
            services.AddMediatR(typeof(ServiceCollectionExtensions));
            return services;
        }
    }
}
