using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Abstractions.Services.Authentication;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.DeleteUser;
using EEaseWebAPI.Application.MapEntities.StatusCheck;
using EEaseWebAPI.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace EEaseWebAPI.Persistence.Services.User
{
    public class UserAccountService : IUserAccountService
    {
        public static readonly TimeSpan DeletionGracePeriod = TimeSpan.FromDays(7);

        private static readonly TimeSpan DeleteCodeLifetime = TimeSpan.FromMinutes(15);

        private readonly UserManager<AppUser> _userManager;
        private readonly IMailService _mailService;
        private readonly IVerificationCodeGenerator _codeGenerator;

        public UserAccountService(
            UserManager<AppUser> userManager,
            IMailService mailService,
            IVerificationCodeGenerator codeGenerator)
        {
            _userManager = userManager;
            _mailService = mailService;
            _codeGenerator = codeGenerator;
        }

        public async Task UpdateRefreshTokenAsync(string refreshToken, AppUser user, TimeSpan lifetime)
        {
            if (user == null)
            {
                throw new Application.Exceptions.Login.UserNotFoundException(
                    "User not found", (int)StatusEnum.UserNotFound);
            }

            user.RefreshToken = refreshToken;
            user.RefreshTokenEndDate = DateTime.UtcNow.Add(lifetime);

            await _userManager.UpdateAsync(user);
        }

        public async Task<DeleteRequestOutcome> RequestDeletionAsync(string username)
        {
            var user = await FindAsync(username);

            if (user.Status == false)
            {
                user.Status = true;
                user.DeleteDate = null;
                user.DeleteCode = null;
                user.DeleteCodeExpiration = null;

                await _userManager.UpdateAsync(user);

                return DeleteRequestOutcome.Reactivated;
            }

            user.DeleteCode = _codeGenerator.Generate();
            user.DeleteCodeExpiration = DateTime.UtcNow.Add(DeleteCodeLifetime);

            await _userManager.UpdateAsync(user);

            _mailService.SendDeleteCodeEmail(user.Email, "Delete Account", user.DeleteCode);

            return DeleteRequestOutcome.CodeSent;
        }

        public async Task<string> ConfirmDeletionAsync(string username, string code)
        {
            var user = await FindAsync(username);

            if (user.Status == false)
            {
                throw new UserStatusAlreadyFalseException(
                    "User status already false", (int)StatusEnum.UserDeletionFailed);
            }

            if (user.DeleteCode == null || user.DeleteCodeExpiration == null)
            {
                throw new DeleteUserCodeNotCorrectException(
                    "Delete user code is wrong.", (int)StatusEnum.InvalidDeleteCode);
            }

            if (user.DeleteCodeExpiration <= DateTime.UtcNow)
            {
                user.DeleteCode = null;
                user.DeleteCodeExpiration = null;
                await _userManager.UpdateAsync(user);

                throw new DeleteUserCodeNotCorrectException(
                    "Delete code has expired. Please request a new one.", (int)StatusEnum.DeleteCodeExpired);
            }

            if (user.DeleteCode != code)
            {
                throw new DeleteUserCodeNotCorrectException(
                    "Delete user code is wrong.", (int)StatusEnum.InvalidDeleteCode);
            }

            user.DeleteDate = DateTime.UtcNow.Add(DeletionGracePeriod);
            user.Status = false;
            user.DeleteCode = null;
            user.DeleteCodeExpiration = null;

            await _userManager.UpdateAsync(user);

            return $"Code is correct, account will be deleted in {DeletionGracePeriod.Days} days";
        }

        public async Task<StatusCheckBody> StatusCheck(string username)
        {
            var user = await FindAsync(username);
            var isActive = user.Status != false;

            return new StatusCheckBody
            {
                status = isActive,
                message = isActive ? "User is active" : "User is passive"
            };
        }

        private async Task<AppUser> FindAsync(string username) =>
            await _userManager.FindByNameAsync(username)
            ?? throw new Application.Exceptions.Login.UserNotFoundException(
                "User not found", (int)StatusEnum.UserNotFound);
    }
}
