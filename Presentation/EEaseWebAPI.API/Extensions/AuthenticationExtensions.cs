using EEaseWebAPI.API.Constants;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.API;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using TokenOptions = EEaseWebAPI.Application.Options.TokenOptions;

namespace EEaseWebAPI.API.Extensions
{
    public static class AuthenticationExtensions
    {
        public const int MinimumSecurityKeyBytes = 32;

        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var tokenOptions = configuration.GetSection(TokenOptions.SectionName).Get<TokenOptions>();

            if (tokenOptions is null ||
                string.IsNullOrWhiteSpace(tokenOptions.Issuer) ||
                string.IsNullOrWhiteSpace(tokenOptions.Audience) ||
                string.IsNullOrWhiteSpace(tokenOptions.SecurityKey))
            {
                throw new InvalidOperationException(
                    "Token settings are missing. Define 'Token:Issuer', 'Token:Audience' and " +
                    "'Token:SecurityKey' in appsettings or in environment variables.");
            }

            // HS256 refuses a key shorter than this, and it refuses it while signing rather
            // than while starting: a short key boots an application that 500s on every login.
            var keySizeInBytes = Encoding.UTF8.GetByteCount(tokenOptions.SecurityKey);

            if (keySizeInBytes < MinimumSecurityKeyBytes)
            {
                throw new InvalidOperationException(
                    $"'Token:SecurityKey' is {keySizeInBytes} bytes. HMAC-SHA256 needs at least " +
                    $"{MinimumSecurityKeyBytes}.");
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

                    // Without this the framework answers a missing or expired token with an
                    // empty body, which is the one answer a caller could not read the same
                    // way as every other.
                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            context.HandleResponse();

                            return WriteAsync(
                                context.HttpContext,
                                StatusCodes.Status401Unauthorized,
                                "Unauthorized",
                                StatusEnum.AuthenticationRequired,
                                "This endpoint needs a signed in caller.");
                        },
                        OnForbidden = context => WriteAsync(
                            context.HttpContext,
                            StatusCodes.Status403Forbidden,
                            "Forbidden",
                            StatusEnum.AccessForbidden,
                            "This caller is not allowed to do that.")
                    };
                });

            services.AddAuthorization();

            return services;
        }

        private static Task WriteAsync(
            HttpContext context, int statusCode, string title, StatusEnum statusEnum, string fallback)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            // The caller reads this in their own language, like every other error.
            var localizer = context.RequestServices.GetService<IStringLocalizer<ErrorMessages>>();
            var translation = localizer?[statusEnum.ToString()];
            var message = translation is null || translation.ResourceNotFound ? fallback : translation.Value;

            return context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Header = new Header
                {
                    Success = false,
                    ResponseDate = DateTime.UtcNow,
                    EnumStatusCode = (int)statusEnum
                },
                Body = new ErrorBody
                {
                    StatusCode = statusCode,
                    Title = title,
                    Message = message
                }
            });
        }
    }
}
