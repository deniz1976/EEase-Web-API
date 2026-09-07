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
            services.AddOptions<TokenOptions>()
                .Bind(configuration.GetSection(TokenOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<DatabaseOptions>()
                .Bind(configuration.GetSection(DatabaseOptions.SectionName))
                .ValidateDataAnnotations();

            services.AddOptions<RateLimitOptions>()
                .Bind(configuration.GetSection(RateLimitOptions.SectionName))
                .ValidateDataAnnotations();

            services.AddOptions<CorsOptions>()
                .Bind(configuration.GetSection(CorsOptions.SectionName));

            services.AddOptions<CacheOptions>()
                .Bind(configuration.GetSection(CacheOptions.SectionName));

            services.AddOptions<MailOptions>()
                .Bind(configuration.GetSection(MailOptions.SectionName))
                .ValidateDataAnnotations();

            services.AddOptions<RedisOptions>()
                .Bind(configuration.GetSection(RedisOptions.SectionName))
                .ValidateDataAnnotations();

            services.AddOptions<GeminiOptions>()
                .Bind(configuration.GetSection(GeminiOptions.SectionName))
                .ValidateDataAnnotations();

            services.AddOptions<GooglePlacesOptions>()
                .Bind(configuration.GetSection(GooglePlacesOptions.SectionName))
                .ValidateDataAnnotations();

            return services;
        }
    }
}
