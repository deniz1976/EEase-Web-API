using System.Security.Claims;
using System.Text;
using EEaseWebAPI.API.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TokenOptions = EEaseWebAPI.Application.Options.TokenOptions;

namespace EEaseWebAPI.API.Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var tokenOptions = configuration.GetSection(TokenOptions.SectionName).Get<TokenOptions>();

            if (tokenOptions is null || string.IsNullOrWhiteSpace(tokenOptions.SecurityKey))
            {
                throw new InvalidOperationException(
                    "Token settings are missing. Define 'Token:Issuer', 'Token:Audience' and " +
                    "'Token:SecurityKey' in appsettings or in environment variables.");
            }

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = AuthenticationSchemes.User;
                    options.DefaultChallengeScheme = AuthenticationSchemes.User;
                    options.DefaultScheme = AuthenticationSchemes.User;
                })
                .AddJwtBearer(AuthenticationSchemes.User, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidAudience = tokenOptions.Audience,
                        ValidIssuer = tokenOptions.Issuer,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(tokenOptions.SecurityKey)),
                        NameClaimType = ClaimTypes.Name,

                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddAuthorization();

            return services;
        }
    }
}
