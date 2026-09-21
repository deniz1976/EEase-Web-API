using EEaseWebAPI.Application.Security;
using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Abstractions.Services.Authentication;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.ChangePassword;
using EEaseWebAPI.Application.Exceptions.ResetPassword;
using EEaseWebAPI.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using EEaseWebAPI.Application;
using EEaseWebAPI.Application.Resources;

namespace EEaseWebAPI.Persistence.Services.Authentication
{
    public sealed class PasswordService : IPasswordService
    {
        private static readonly TimeSpan ResetCodeLifetime = TimeSpan.FromMinutes(15);
        private const int MaxResetCodeAttempts = 5;

        private readonly UserManager<AppUser> _userManager;
        private readonly IMailService _mailService;
        private readonly IVerificationCodeGenerator _codeGenerator;
        private readonly ILogger<PasswordService> _logger;

        public PasswordService(
            UserManager<AppUser> userManager,
            IMailService mailService,
            IVerificationCodeGenerator codeGenerator,
            ILogger<PasswordService> logger)
        {
            _userManager = userManager;
            _mailService = mailService;
            _codeGenerator = codeGenerator;
            _logger = logger;
        }

        public async Task<bool> SendResetCodeAsync(string usernameOrEmail, CancellationToken cancellationToken = default)
        {
            // A name nobody has registered is answered the same way a real one is. Saying
            // "no such user" here is how a stranger finds out who has an account.
            var user = await FindOrDefaultAsync(usernameOrEmail);

            if (user is null)
            {
                return true;
            }

            var code = _codeGenerator.Generate();

            user.ResetPasswordCode = code;
            user.ResetPasswordCodeExpiration = DateTime.UtcNow.Add(ResetCodeLifetime);
            user.ResetPasswordCodeAttempts = 0;

            await UpdateAsync(user, "Failed to store the reset password code.");

            var sent = await _mailService.SendResetPasswordEmailAsync(
                user.Email!, AppMessages.Mail_ResetPasswordSubject, code, cancellationToken);

            if (!sent)
            {
                // Reporting the failure here would answer differently for a name that exists
                // and one that does not, which is the hole this endpoint is usually found
                // with. The caller is told the same thing either way and can ask again; the
                // operator is the one who needs to know, and is told here.
                _logger.LogError(
                    "A reset code was stored for {UserId} but could not be mailed. " +
                    "The caller was answered as though it went out.",
                    user.Id);
            }

            return true;
        }

        public async Task<bool> VerifyResetCodeAsync(string usernameOrEmail, string code, CancellationToken cancellationToken = default)
        {
            var user = await FindAsync(usernameOrEmail);

            await ValidateResetCodeAsync(user, code);

            return true;
        }

        public async Task ResetPasswordAsync(string usernameOrEmail, string code, string newPassword, CancellationToken cancellationToken = default)
        {
            var user = await FindAsync(usernameOrEmail);

            await ValidateResetCodeAsync(user, code);

            if (user.PasswordHash != null &&
                _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, newPassword)
                    != PasswordVerificationResult.Failed)
            {
                throw new PasswordChangeException(
                    "Old password and new password are the same.", (int)StatusEnum.PasswordChangeFailed);
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

            if (!result.Succeeded)
            {
                throw new PasswordChangeException(
                    $"Failed to reset the password. Errors: {Describe(result)}",
                    (int)StatusEnum.ResetPasswordFailed);
            }

            ClearResetCode(user);

            await UpdateAsync(user, "Failed to clear the reset password code.");
        }

        public async Task ChangePasswordAsync(string username, string oldPassword, string newPassword, CancellationToken cancellationToken = default)
        {
            if (oldPassword == newPassword)
            {
                throw new SamePasswordsException(
                    "Old password and new password are the same.", (int)StatusEnum.PasswordChangeFailed);
            }

            var user = await FindAsync(username);

            if (!await _userManager.CheckPasswordAsync(user, oldPassword))
            {
                throw new InvalidPasswordException(
                    "The old password is incorrect.", (int)StatusEnum.InvalidPassword);
            }

            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

            if (!result.Succeeded)
            {
                throw new PasswordChangeException(
                    $"Failed to change the password. Errors: {Describe(result)}",
                    (int)StatusEnum.PasswordChangeFailed);
            }
        }

        private async Task ValidateResetCodeAsync(AppUser user, string code)
        {
            if (user.ResetPasswordCode == null || user.ResetPasswordCodeExpiration == null)
            {
                throw new ResetPasswordCodeNotCorrectException(
                    "Reset password code is not correct", (int)StatusEnum.InvalidResetPasswordCode);
            }

            if (user.ResetPasswordCodeExpiration <= DateTime.UtcNow ||
                user.ResetPasswordCodeAttempts >= MaxResetCodeAttempts)
            {
                ClearResetCode(user);
                await UpdateAsync(user, "Failed to clear the reset password code.");

                throw new ResetPasswordCodeExpiredException(
                    "Reset password code has expired. Please request a new one.",
                    (int)StatusEnum.ResetPasswordCodeExpired);
            }

            if (!SecretCode.Matches(user.ResetPasswordCode, code))
            {
                user.ResetPasswordCodeAttempts++;
                await UpdateAsync(user, "Failed to record the reset password attempt.");

                throw new ResetPasswordCodeNotCorrectException(
                    "Reset password code is not correct", (int)StatusEnum.InvalidResetPasswordCode);
            }
        }

        private static void ClearResetCode(AppUser user)
        {
            user.ResetPasswordCode = null;
            user.ResetPasswordCodeExpiration = null;
            user.ResetPasswordCodeAttempts = 0;
        }

        private async Task<AppUser> FindAsync(string usernameOrEmail) =>
            await FindOrDefaultAsync(usernameOrEmail)
            ?? throw new Application.Exceptions.Login.UserNotFoundException(
                "User not found", (int)StatusEnum.UserNotFound);

        private async Task<AppUser?> FindOrDefaultAsync(string usernameOrEmail) =>
            await _userManager.FindByNameAsync(usernameOrEmail)
            ?? await _userManager.FindByEmailAsync(usernameOrEmail);

        private async Task UpdateAsync(AppUser user, string failureMessage)
        {
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new PasswordChangeException(
                    $"{failureMessage} Errors: {Describe(result)}", (int)StatusEnum.PasswordChangeFailed);
            }
        }

        private static string Describe(IdentityResult result) =>
            string.Join("; ", result.Errors.Select(error => error.Description));
    }
}
