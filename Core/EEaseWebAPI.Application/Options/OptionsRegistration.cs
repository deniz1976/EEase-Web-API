using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EEaseWebAPI.Application.Options
{
    public static class OptionsRegistration
    {
        public static IServiceCollection AddApplicationOptions(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .Register<TokenOptions>(configuration, TokenOptions.SectionName)
                .Register<DatabaseOptions>(configuration, DatabaseOptions.SectionName)
                .Register<RateLimitOptions>(configuration, RateLimitOptions.SectionName)
                .Register<CorsOptions>(configuration, CorsOptions.SectionName)
                .Register<CacheOptions>(configuration, CacheOptions.SectionName)
                .Register<MailOptions>(configuration, MailOptions.SectionName)
                .Register<RedisOptions>(configuration, RedisOptions.SectionName)
                .Register<GeminiOptions>(configuration, GeminiOptions.SectionName)
                .Register<GooglePlacesOptions>(configuration, GooglePlacesOptions.SectionName);

            return services;
        }

        private static IServiceCollection Register<TOptions>(
            this IServiceCollection services, IConfiguration configuration, string sectionName)
            where TOptions : class
        {
            services.AddOptions<TOptions>()
                .Bind(configuration.GetSection(sectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            return services;
        }
    }
}
