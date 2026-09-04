using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Infrastructure.Services;
using EEaseWebAPI.Infrastructure.Services.Token;
using Microsoft.Extensions.DependencyInjection;
using ITokenHandler = EEaseWebAPI.Application.Abstractions.Token.ITokenHandler;

namespace EEaseWebAPI.Infrastructure
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<ITokenHandler, TokenHandler>();

            services.AddSingleton<MailTemplateProvider>();
            services.AddScoped<IMailService, MailService>();

            services.AddHttpClient();
            services.AddScoped<IHttpService, HttpService>();

            return services;
        }
    }
}
