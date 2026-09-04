using System.Reflection;
using EEaseWebAPI.Application.Behaviors;
using EEaseWebAPI.Application.Options;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EEaseWebAPI.Application
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var applicationAssembly = Assembly.GetExecutingAssembly();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(applicationAssembly);
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            services.AddValidatorsFromAssembly(applicationAssembly, includeInternalTypes: true);
            services.AddAutoMapper(config => config.AddMaps(applicationAssembly));
            services.AddApplicationOptions(configuration);

            return services;
        }
    }
}
