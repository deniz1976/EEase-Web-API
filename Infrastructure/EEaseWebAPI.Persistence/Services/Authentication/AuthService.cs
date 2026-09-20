using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Abstractions.Services.Authentication;
using EEaseWebAPI.Application.Abstractions.Token;
using EEaseWebAPI.Application.DTOs;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.MapEntities.Login;
using EEaseWebAPI.Application.MapEntities.RefreshTokenLogin;
using EEaseWebAPI.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EEaseWebAPI.Application;
using EEaseWebAPI.Application.Resources;

namespace EEaseWebAPI.Persistence.Services.Authentication
{
    public class AuthService : IAuthService
    {
        private const int RefreshedAccessTokenLifetime = 15 * 60;
        private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);
        private const int UsernameChangeTokenLifetime = 24 * 60 * 60;

        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenHandler _tokenHandler;
        private readonly IMailService _mailService;
        private readonly IUserAccountService _accountService;
        private readonly IAccountDeletionPolicy _deletionPolicy;
        private readonly IVerificationCodeGenerator _codeGenerator;

        public AuthService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenHandler tokenHandler,
            IMailService mailService,
            IUserAccountService accountService,
            IAccountDeletionPolicy deletionPolicy,
            IVerificationCodeGenerator codeGenerator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenHandler = tokenHandler;
            _mailService = mailService;
            _accountService = accountService;
            _deletionPolicy = deletionPolicy;
            _codeGenerator = codeGenerator;
        }

        public async Task<LoginBody> LoginAsync(string usernameOrEmail, string password, int accessTokenLifetime, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByNameAsync(usernameOrEmail)
                       ?? await _userManager.FindByEmailAsync(usernameOrEmail)
                       ?? throw new Application.Exceptions.Login.UserNotFoundException("User not found", (int)StatusEnum.UserNotFound);

            var signIn = await _signInManager.CheckPasswordSignInAsync(user, password, false);

            if (!signIn.Succeeded)
            {
                throw new Application.Exceptions.Login.UserAuthenticationException(
                    "Username or password is incorrect", (int)StatusEnum.InvalidCredentials);
            }

            if (!user.EmailConfirmed)
            {
                await SendVerificationEmailAsync(user);

                throw new Application.Exceptions.EmailConfirmException(
                    "Email confirmation required, new code sent to email.", (int)StatusEnum.EmailNotConfirmed);
            }

            var status = await _deletionPolicy.EnforceAsync(user, cancellationToken);

            if (status.IsDeleted)
            {
                throw new Application.Exceptions.Login.UserNotFoundException(status.Message, (int)StatusEnum.UserNotFound);
            }

            var previousLastSeen = user.LastSeen;

            var token = _tokenHandler.CreateAccessToken(accessTokenLifetime, user);
            await _accountService.UpdateRefreshTokenAsync(token.RefreshToken, user, RefreshTokenLifetime, cancellationToken);

            user.LastSeen = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return new LoginBody
            {
                Token = token,
                Warning = status.Message,
                UserInfo = new UserInfo
                {
                    Name = user.Name,
                    Surname = user.Surname,
                    Gender = user.Gender,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    Country = user.Country,
                    Status = user.Status,
                    DeleteDate = user.DeleteDate,
                    BornDate = user.BornDate,
                    Username = user.UserName,
                    Bio = user.Bio,
                    Currency = user.Currency,
                    PhotoPath = user.PhotoPath,
                    LastSeen = previousLastSeen
                }
            };
        }

        public async Task<RefreshTokenLoginBody> RefreshTokenLoginAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(candidate => candidate.RefreshToken == refreshToken, cancellationToken)
                       ?? throw new Application.Exceptions.Login.UserNotFoundException("User Not Found.", (int)StatusEnum.UserNotFound);

            if (user.RefreshTokenEndDate <= DateTime.UtcNow)
            {
                throw new Application.Exceptions.Login.UserNotFoundException(
                    "Refresh token has expired or is invalid.", (int)StatusEnum.RefreshTokenExpired);
            }

            var status = await _deletionPolicy.EnforceAsync(user, cancellationToken);

            if (status.IsDeleted)
            {
                return new RefreshTokenLoginBody { Token = null, Warning = status.Message };
            }

            var token = _tokenHandler.CreateAccessToken(RefreshedAccessTokenLifetime, user);
            await _accountService.UpdateRefreshTokenAsync(token.RefreshToken, user, RefreshTokenLifetime, cancellationToken);

            user.LastSeen = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return new RefreshTokenLoginBody { Token = token, Warning = status.Message };
        }

        public async Task<Token> UpdateUserGetNewToken(string newUsername, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(newUsername) ||
                await _userManager.FindByNameAsync(newUsername) is not AppUser user)
            {
                throw new Application.Exceptions.Login.UserNotFoundException("User not found", (int)StatusEnum.UserNotFound);
            }

            var token = _tokenHandler.CreateAccessToken(UsernameChangeTokenLifetime, user);
            await _accountService.UpdateRefreshTokenAsync(token.RefreshToken, user, RefreshTokenLifetime, cancellationToken);

            return token;
        }

        public async Task<bool> IsEmailInUse(string email, CancellationToken cancellationToken = default) =>
            await _userManager.FindByEmailAsync(email) != null;

        private async Task SendVerificationEmailAsync(AppUser user)
        {
            user.VerificationCode = _codeGenerator.Generate();

            await _userManager.UpdateAsync(user);

            // A login that needs confirming is answered the same way whether or not the mail
            // went out; the caller can ask for the code again.
            await _mailService.SendVerificationEmailAsync(
                user.Email!, AppMessages.Mail_VerificationSubject, user.VerificationCode);
        }
    }
}
