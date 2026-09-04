using EEaseWebAPI.API.Constants;
using CorsOptions = EEaseWebAPI.Application.Options.CorsOptions;

namespace EEaseWebAPI.API.Extensions
{
    public static class CorsExtensions
    {
        public static IServiceCollection AddApiCors(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            var corsOptions = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>()
                              ?? new CorsOptions();

            services.AddCors(options => options.AddPolicy(CorsPolicies.Default, policy =>
            {
                if (corsOptions.AllowedOrigins.Count > 0)
                {
                    policy.WithOrigins(corsOptions.AllowedOrigins.ToArray())
                        .AllowAnyHeader()
                        .AllowAnyMethod();

                    if (corsOptions.AllowCredentials)
                    {
                        policy.AllowCredentials();
                    }

                    return;
                }

                if (environment.IsDevelopment())
                {
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                }
            }));

            return services;
        }
    }
}
