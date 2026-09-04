using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Abstractions.Services.Authentication;
using EEaseWebAPI.Application.Options;
using EEaseWebAPI.Application.Repositories;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Contexts;
using EEaseWebAPI.Persistence.Repositories;
using EEaseWebAPI.Persistence.Services;
using EEaseWebAPI.Persistence.Services.Gemini;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EEaseWebAPI.Persistence
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddPersistenceServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddMemoryCache();
            services.AddDatabase(configuration);
            services.AddIdentityCore();
            services.AddRepositories();
            services.AddDomainServices();
            services.AddExternalApiClients();

            return services;
        }

        private static IServiceCollection AddDatabase(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("PostgreSQL");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "No PostgreSQL connection string was found. Set 'ConnectionStrings:PostgreSQL' " +
                    "in appsettings, in user secrets, or through the " +
                    "'ConnectionStrings__PostgreSQL' environment variable.");
            }

            var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
                                  ?? new DatabaseOptions();

            services.AddDbContext<EEaseAPIDbContext>(options =>
            {
                options.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.CommandTimeout(databaseOptions.CommandTimeoutSeconds);
                    npgsql.EnableRetryOnFailure(
                        maxRetryCount: databaseOptions.MaxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(databaseOptions.MaxRetryDelaySeconds),
                        errorCodesToAdd: null);
                });

                if (databaseOptions.EnableSensitiveDataLogging)
                {
                    options.EnableSensitiveDataLogging();
                }
            });

            return services;
        }

        private static IServiceCollection AddIdentityCore(this IServiceCollection services)
        {
            services
                .AddIdentity<AppUser, AppRole>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Lockout.MaxFailedAccessAttempts = 10;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                })
                .AddEntityFrameworkStores<EEaseAPIDbContext>()
                .AddDefaultTokenProviders();

            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IReadRepository<>), typeof(ReadRepository<>));
            services.AddScoped(typeof(IWriteRepository<>), typeof(WriteRepository<>));
            services.AddScoped<IStandardRouteReadRepository, StandardRouteReadRepository>();
            services.AddScoped<IStandardRouteWriteRepository, StandardRouteWriteRepository>();
            services.AddScoped<IAllWorldCitiesRepository, AllWorldCitiesRepository>();
            services.AddScoped<IUserAccommodationPreferencesReadRepository, UserAccommodationPreferencesReadRepository>();
            services.AddScoped<IUserAccommodationPreferencesWriteRepository, UserAccommodationPreferencesWriteRepository>();
            services.AddScoped<IUserFoodPreferencesReadRepository, UserFoodPreferencesReadRepository>();
            services.AddScoped<IUserFoodPreferencesWriteRepository, UserFoodPreferencesWriteRepository>();
            services.AddScoped<IUserPersonalizationReadRepository, UserPersonalizationReadRepository>();
            services.AddScoped<IUserPersonalizationWriteRepository, UserPersonalizationWriteRepository>();

            return services;
        }

        private static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IExternalAuthentication, AuthService>();
            services.AddScoped<IInternalAuthentication, AuthService>();
            services.AddScoped<IHeaderService, HeaderService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<ICityService, CityService>();
            services.AddScoped<IRouteAccessPolicy, RouteAccessPolicy>();
            services.AddScoped<ISystemUserProvider, SystemUserProvider>();
            services.AddScoped<IRouteService, RouteService>();
            services.AddScoped<ICustomRouteService, CustomRouteService>();
            services.AddScoped<IUserCacheService, UserCacheService>();
            services.AddScoped<PasswordHasher<string>>();

            services.AddHostedService<CacheInitializationService>();

            return services;
        }

        private static IServiceCollection AddExternalApiClients(this IServiceCollection services)
        {
            services.AddSingleton<IGeminiKeyManager, GeminiKeyManager>();

            services.AddHttpClient<IGeminiApiClient, GeminiApiClient>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<GeminiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseAddress);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

            services.AddScoped<IGeminiAIService, GeminiAIService>();

            services.AddHttpClient<IGooglePlacesService, GooglePlacesService>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<GooglePlacesOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseAddress);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

            return services;
        }
    }
}
