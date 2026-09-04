using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EEaseWebAPI.Application.Abstractions.Token;
using EEaseWebAPI.Domain.Entities.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TokenOptions = EEaseWebAPI.Application.Options.TokenOptions;

namespace EEaseWebAPI.Infrastructure.Services.Token
{
    public sealed class TokenHandler : ITokenHandler
    {
        private readonly TokenOptions _options;

        public TokenHandler(IOptions<TokenOptions> options)
        {
            _options = options.Value;
        }

        public Application.DTOs.Token CreateAccessToken(int seconds, AppUser user)
        {
            ArgumentNullException.ThrowIfNull(user);

            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                throw new InvalidOperationException(
                    "Cannot issue an access token because the user name is empty.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecurityKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddSeconds(seconds);

            var securityToken = new JwtSecurityToken(
                audience: _options.Audience,
                issuer: _options.Issuer,
                expires: expiration,
                notBefore: DateTime.UtcNow,
                signingCredentials: signingCredentials,
                claims: new List<Claim>
                {
                    new(ClaimTypes.Name, user.UserName),
                    new(ClaimTypes.NameIdentifier, user.Id),
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                });

            return new Application.DTOs.Token
            {
                Expiration = expiration,
                AccessToken = new JwtSecurityTokenHandler().WriteToken(securityToken),
                RefreshToken = CreateRefreshToken()
            };
        }

        public string CreateRefreshToken()
        {
            var number = new byte[32];
            RandomNumberGenerator.Fill(number);
            return Convert.ToBase64String(number);
        }
    }
}
