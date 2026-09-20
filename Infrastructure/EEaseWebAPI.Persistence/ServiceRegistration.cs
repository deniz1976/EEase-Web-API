using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Abstractions.Services.Authentication;
using EEaseWebAPI.Application.Options;
using EEaseWebAPI.Application.Repositories;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Contexts;
using EEaseWebAPI.Persistence.Repositories;
using EEaseWebAPI.Persistence.Services;
using EEaseWebAPI.Persistence.Services.Gemini;
using EEaseWebAPI.Persistence.Services.Route;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

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
            services.AddExternalApiClients(configuration);

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
            services.AddScoped<IAllWorldCitiesRepository, AllWorldCitiesRepository>();

            return services;
        }

        private static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            services.AddScoped<IUserRegistrationService, Services.User.UserRegistrationService>();
            services.AddScoped<IUserProfileService, Services.User.UserProfileService>();
            services.AddScoped<IUserAccountService, Services.User.UserAccountService>();
            services.AddScoped<IFriendshipService, Services.Social.FriendshipService>();
            services.AddScoped<IUserPreferenceService, Services.User.UserPreferenceService>();
            services.AddScoped<IAuthService, Services.Authentication.AuthService>();
            services.AddScoped<IInternalAuthentication, Services.Authentication.AuthService>();
            services.AddScoped<IPasswordService, Services.Authentication.PasswordService>();
            services.AddScoped<IAccountDeletionPolicy, Services.Authentication.AccountDeletionPolicy>();
            services.AddSingleton<IVerificationCodeGenerator, Services.Authentication.VerificationCodeGenerator>();
            services.AddScoped<IHeaderService, HeaderService>();
            services.AddScoped<ICurrencyService, Services.ReferenceData.CurrencyService>();
            services.AddScoped<ICityService, Services.ReferenceData.CityService>();
            services.AddScoped<Services.Caching.ReferenceDataCache>();
            services.AddScoped<IRouteAccessPolicy, Services.Route.RouteAccessPolicy>();
            services.AddScoped<ISystemUserProvider, Services.Route.SystemUserProvider>();
            services.AddSingleton<IRoutePlanValidator, RoutePlanValidator>();
            services.AddScoped<IPlaceSearchService, PlaceSearchService>();
            services.AddScoped<IPlaceSelectionService, PlaceSelectionService>();
            services.AddScoped<IRouteEnrichmentService, RouteEnrichmentService>();
            services.AddSingleton(Random.Shared);
            services.AddScoped<IPreferenceFeedbackService, Services.Route.PreferenceFeedbackService>();
            services.AddScoped<IPlaceReplacementService, Services.Route.PlaceReplacementService>();
            services.AddScoped<IDislikedPlaceService, Services.Route.DislikedPlaceService>();
            services.AddSingleton<IPlaceQueryBuilder, Services.Route.PlaceQueryBuilder>();
            services.AddSingleton<IPreferenceProfileBuilder, Services.Route.PreferenceProfileBuilder>();
            services.AddScoped<IRouteQueryService, Services.Route.RouteQueryService>();
            services.AddScoped<IRouteInteractionService, Services.Route.RouteInteractionService>();
            services.AddScoped<IRouteDislikeService, Services.Route.RouteDislikeService>();
            services.AddScoped<IRouteService, Services.Route.RouteService>();
            services.AddScoped<ITravellerPreferenceCollector, Services.Route.TravellerPreferenceCollector>();
            services.AddScoped<IRandomRouteBuilder, Services.Route.RandomRouteBuilder>();
            services.AddScoped<IPreferenceRouteBuilder, Services.Route.PreferenceRouteBuilder>();
            services.AddScoped<ICustomRouteService, Services.Route.CustomRouteService>();
            services.AddScoped<IUserCacheService, Services.Caching.UserCacheService>();

            services.AddHostedService<Services.Caching.CacheInitializationService>();

            return services;
        }

        private static IServiceCollection AddExternalApiClients(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddGeminiKeyManager(configuration);

            services.AddHttpClient<IGeminiApiClient, GeminiApiClient>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<GeminiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseAddress);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

            services.AddScoped<IGeminiAIService, GeminiAIService>();

            services.AddHttpClient<IGooglePlacesService, Services.GooglePlaces.GooglePlacesService>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<GooglePlacesOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseAddress);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

            return services;
        }

        private static IServiceCollection AddGeminiKeyManager(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>()
                               ?? new RedisOptions();

            services.AddSingleton<GeminiKeyManager>();

            if (!redisOptions.IsEnabled)
            {
                services.AddSingleton<IGeminiKeyManager>(
                    provider => provider.GetRequiredService<GeminiKeyManager>());

                return services;
            }

            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var configurationOptions = ConfigurationOptions.Parse(redisOptions.ConnectionString);

                configurationOptions.AbortOnConnectFail = false;
                configurationOptions.ConnectTimeout = redisOptions.ConnectTimeoutSeconds * 1000;

                return ConnectionMultiplexer.Connect(configurationOptions);
            });

            services.AddSingleton<IGeminiKeyManager, RedisGeminiKeyManager>();

            return services;
        }
    }
}
