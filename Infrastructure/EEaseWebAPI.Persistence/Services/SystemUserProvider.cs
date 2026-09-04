using System.Security.Cryptography;
using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EEaseWebAPI.Persistence.Services
{
    public sealed class SystemUserProvider : ISystemUserProvider
    {
        public const string SystemUserName = "admin";

        private const string SystemUserEmail = "system@eease.local";

        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<SystemUserProvider> _logger;

        public SystemUserProvider(UserManager<AppUser> userManager, ILogger<SystemUserProvider> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<AppUser> GetOrCreateAsync()
        {
            var existing = await _userManager.FindByNameAsync(SystemUserName);

            if (existing is not null)
            {
                return existing;
            }

            var systemUser = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = SystemUserName,
                Email = SystemUserEmail,
                EmailConfirmed = true,
                Name = "EEase",
                Surname = "System",
                Status = true,

                LockoutEnabled = true,
                LockoutEnd = DateTimeOffset.MaxValue
            };

            var result = await _userManager.CreateAsync(systemUser, GenerateUnusablePassword());

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Could not create the system account: {errors}");
            }

            _logger.LogInformation(
                "Created the '{UserName}' system account that owns anonymous routes.",
                SystemUserName);

            return systemUser;
        }

        private static string GenerateUnusablePassword() =>
            $"{Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))}aA1!";
    }
}
