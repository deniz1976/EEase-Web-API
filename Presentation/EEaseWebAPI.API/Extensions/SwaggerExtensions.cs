using EEaseWebAPI.API.Constants;
using EEaseWebAPI.Application.JsonConverters;
using Microsoft.OpenApi.Models;

namespace EEaseWebAPI.API.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "EEase Web API",
                    Version = "v1",
                    Description = "Travel itinerary planning and social interaction services."
                });

                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT access token. Enter the token value only; the 'Bearer' prefix is added for you."
                };

                options.AddSecurityDefinition(AuthenticationSchemes.User, securityScheme);

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = AuthenticationSchemes.User
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                options.SchemaFilter<DateOnlySchemaFilter>();
            });

            return services;
        }

        public static WebApplication UseSwaggerDocumentation(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "EEase Web API v1");
                options.DocumentTitle = "EEase Web API";
            });

            return app;
        }
    }
}
