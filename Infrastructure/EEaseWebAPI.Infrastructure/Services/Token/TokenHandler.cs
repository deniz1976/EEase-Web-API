using EEaseWebAPI.Application.Abstractions.Token;
using EEaseWebAPI.Domain.Entities.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Infrastructure.Services.Token
{
    public class TokenHandler : ITokenHandler
    {
        private readonly IConfiguration _configuraton;

        public TokenHandler(IConfiguration configuraton)
        {
            _configuraton = configuraton;
        }
    
        public Application.DTOs.Token CreateAccessToken(int seconds, AppUser user)
        {
            Application.DTOs.Token token = new Application.DTOs.Token();

            SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(_configuraton["Token:SecurityKey"]));

            SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256);

            

            token.Expiration = DateTime.UtcNow.AddSeconds(seconds);
            JwtSecurityToken securityToken = new
                (
                audience: _configuraton["Token:Audience"],
                issuer  : _configuraton["Token:Issuer"],
                expires : token.Expiration,
                notBefore : DateTime.UtcNow,
                signingCredentials : signingCredentials,
                claims: new List<Claim> { new(ClaimTypes.Name, user?.UserName) }
                );

            JwtSecurityTokenHandler tokenHandler = new();
            token.AccessToken = tokenHandler.WriteToken( securityToken );

            token.RefreshToken = CreateRefreshToken();
            return token;
        }

        public string CreateRefreshToken()
        {
            byte[] number = new byte[32];
            using RandomNumberGenerator random = RandomNumberGenerator.Create();

            random.GetBytes(number);
            return Convert.ToBase64String(number);
        }
    }
}
