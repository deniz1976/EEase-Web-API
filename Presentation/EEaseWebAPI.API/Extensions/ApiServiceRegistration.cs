using EEaseWebAPI.Application.JsonConverters;
using EEaseWebAPI.Infrastructure.Filters;
using EEaseWebAPI.Persistence.Contexts;

namespace EEaseWebAPI.API.Extensions
{
    public static class ApiServiceRegistration
    {
        public static IServiceCollection AddApiServices(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            services.AddHttpContextAccessor();

            services
                .AddControllers(options => options.Filters.Add<ValidationFilter>())
                .ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true)
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
                    options.JsonSerializerOptions.ReferenceHandler =
                        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.MaxDepth = 64;
                });

            services.AddJwtAuthentication(configuration);
            services.AddApiCors(configuration, environment);
            services.AddApiRateLimiting(configuration);
            services.AddSwaggerDocumentation();

            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            services.AddHealthChecks()
                .AddDbContextCheck<EEaseAPIDbContext>("database");

            return services;
        }
    }
}
