using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EEaseWebAPI.Application.Repositories;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Contexts;
using EEaseWebAPI.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Persistence.Services;
using EEaseWebAPI.Application.Abstractions.Services.Authentication;

namespace EEaseWebAPI.Persistence
{
    public static class ServiceRegistration
    {
        public static void AddPersistenceServices(this IServiceCollection services, IConfiguration _configuration) 
        {
            services.AddMemoryCache();
            
            services.AddDbContext<EEaseAPIDbContext>(options => 
            {
                options.UseNpgsql(_configuration["ConnectionStrings:PostgreSQL"], npgsqlOptions =>
                {
                    npgsqlOptions.CommandTimeout(300);  
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                });
                options.EnableSensitiveDataLogging(); 
            });

            
            

            /*Repositories*/
            #region 
            services.AddScoped(typeof(IReadRepository<>), typeof(ReadRepository<>));
            services.AddScoped(typeof(IWriteRepository<>), typeof(WriteRepository<>));
            services.AddScoped<IStandardRouteReadRepository,StandardRouteReadRepository>();
            services.AddScoped<IStandardRouteWriteRepository, StandardRouteWriteRepository>();
            services.AddScoped<IAllWorldCitiesRepository, AllWorldCitiesRepository>();
            services.AddScoped<IUserAccommodationPreferencesReadRepository, UserAccommodationPreferencesReadRepository>();
            services.AddScoped<IUserAccommodationPreferencesWriteRepository, UserAccommodationPreferencesWriteRepository>();
            services.AddScoped<IUserFoodPreferencesReadRepository, UserFoodPreferencesReadRepository>();
            services.AddScoped<IUserFoodPreferencesWriteRepository, UserFoodPreferencesWriteRepository>();
            services.AddScoped<IUserPersonalizationReadRepository, UserPersonalizationReadRepository>();
            services.AddScoped<IUserPersonalizationWriteRepository, UserPersonalizationWriteRepository>();
            #endregion

            /*Identity*/
            #region
            services.AddIdentity<AppUser, AppRole>().AddEntityFrameworkStores<EEaseAPIDbContext>().AddDefaultTokenProviders() ;
            #endregion

            /*Services*/
            #region
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IExternalAuthentication, AuthService>();
            services.AddScoped<IInternalAuthentication, AuthService>();
            services.AddScoped<IHeaderService,HeaderService>();
            services.AddHttpClient();
            services.AddScoped<PasswordHasher<string>>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<ICityService, CityService>();
            services.AddHostedService<CacheInitializationService>();
            services.AddScoped<IRouteService, RouteService>();
            services.AddScoped<ICustomRouteService,CustomRouteService>();

            /*Google Places Service Registration*/
            services.AddHttpClient<IGooglePlacesService, GooglePlacesService>();
            services.AddScoped<IGooglePlacesService, GooglePlacesService>();

            /*Gemini AI Services*/
            services.AddSingleton<IGeminiKeyManager, GeminiKeyManager>();
            services.AddScoped<IGeminiAIService, GeminiAIService>();

            services.AddScoped<IUserCacheService, UserCacheService>();
            #endregion
        }
    }
}
