using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Abstractions.Services.Authentication;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.ChangePassword;
using EEaseWebAPI.Application.Exceptions.ResetPassword;
using EEaseWebAPI.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace EEaseWebAPI.Persistence.Services.Authentication
{
    public sealed class PasswordService : IPasswordService
    {
        private static readonly TimeSpan ResetCodeLifetime = TimeSpan.FromMinutes(15);
        private const int MaxResetCodeAttempts = 5;

        private readonly UserManager<AppUser> _userManager;
        private readonly IMailService _mailService;
        private readonly IVerificationCodeGenerator _codeGenerator;

        public PasswordService(
            UserManager<AppUser> userManager,
            IMailService mailService,
            IVerificationCodeGenerator codeGenerator)
        {
            _userManager = userManager;
            _mailService = mailService;
            _codeGenerator = codeGenerator;
        }

        public async Task<bool> SendResetCodeAsync(string usernameOrEmail)
        {
            var user = await FindAsync(usernameOrEmail);

            var code = _codeGenerator.Generate();

            user.ResetPasswordCode = code;
            user.ResetPasswordCodeExpiration = DateTime.UtcNow.Add(ResetCodeLifetime);
            user.ResetPasswordCodeAttempts = 0;

            await UpdateAsync(user, "Failed to store the reset password code.");

            _mailService.SendResetPasswordEmail(user.Email, "Reset Password", code);

            return true;
        }

        public async Task<bool> VerifyResetCodeAsync(string usernameOrEmail, string code)
        {
            var user = await FindAsync(usernameOrEmail);

            await ValidateResetCodeAsync(user, code);

            return true;
        }

        public async Task ResetPasswordAsync(string usernameOrEmail, string code, string newPassword)
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

        public async Task<string> ChangePasswordAsync(string username, string oldPassword, string newPassword)
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

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                return "The old password is correct. Please provide a new password.";
            }

            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

            if (!result.Succeeded)
            {
                throw new PasswordChangeException(
                    $"Failed to change the password. Errors: {Describe(result)}",
                    (int)StatusEnum.PasswordChangeFailed);
            }

            return "Password changed successfully.";
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

            if (user.ResetPasswordCode != code)
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
            await _userManager.FindByNameAsync(usernameOrEmail)
            ?? await _userManager.FindByEmailAsync(usernameOrEmail)
            ?? throw new Application.Exceptions.Login.UserNotFoundException(
                "User not found", (int)StatusEnum.UserNotFound);

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
