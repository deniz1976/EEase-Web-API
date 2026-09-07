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

        public async Task<Token> CreateUserExternalAsync(
            AppUser user, string email, string name, UserLoginInfo info,
            int accessTokenLifetime, string surname, string username, string gender)
        {
            user ??= await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new AppUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = email,
                    Name = name,
                    UserName = username,
                    Gender = gender,
                    Surname = surname
                };

                var creation = await _userManager.CreateAsync(user);

                if (!creation.Succeeded)
                {
                    throw new Application.Exceptions.CreateUser.CreateUserFailedException(
                        "Invalid external authentication.", (int)StatusEnum.CreateUserFailed);
                }
            }

            await _userManager.AddLoginAsync(user, info);

            return _tokenHandler.CreateAccessToken(accessTokenLifetime, user);
        }

        public Task<Token> GoogleLoginAsync(string idToken, int accessTokenLifeTime)
        {
            throw new NotImplementedException();
        }

        public async Task<LoginBody> LoginAsync(string usernameOrEmail, string password, int accessTokenLifetime)
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

            var status = await _deletionPolicy.EnforceAsync(user);

            if (status.IsDeleted)
            {
                throw new Application.Exceptions.Login.UserNotFoundException(status.Message, (int)StatusEnum.UserNotFound);
            }

            var previousLastSeen = user.LastSeen;

            var token = _tokenHandler.CreateAccessToken(accessTokenLifetime, user);
            await _accountService.UpdateRefreshTokenAsync(token.RefreshToken, user, RefreshTokenLifetime);

            user.LastSeen = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return new LoginBody
            {
                token = token,
                warning = status.Message,
                userInfo = new UserInfo
                {
                    name = user.Name,
                    surname = user.Surname,
                    gender = user.Gender,
                    email = user.Email,
                    emailConfirmed = user.EmailConfirmed,
                    country = user.Country,
                    status = user.Status,
                    deleteDate = user.DeleteDate,
                    bornDate = user.BornDate,
                    username = user.UserName,
                    bio = user.Bio,
                    currency = user.Currency,
                    photoPath = user.PhotoPath,
                    lastSeen = previousLastSeen
                }
            };
        }

        public async Task<RefreshTokenLoginBody> RefreshTokenLoginAsync(string refreshToken)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(candidate => candidate.RefreshToken == refreshToken)
                       ?? throw new Application.Exceptions.Login.UserNotFoundException("User Not Found.", (int)StatusEnum.UserNotFound);

            if (user.RefreshTokenEndDate <= DateTime.UtcNow)
            {
                throw new Application.Exceptions.Login.UserNotFoundException(
                    "Refresh token has expired or is invalid.", (int)StatusEnum.RefreshTokenExpired);
            }

            var status = await _deletionPolicy.EnforceAsync(user);

            if (status.IsDeleted)
            {
                return new RefreshTokenLoginBody { Token = null, warning = status.Message };
            }

            var token = _tokenHandler.CreateAccessToken(RefreshedAccessTokenLifetime, user);
            await _accountService.UpdateRefreshTokenAsync(token.RefreshToken, user, RefreshTokenLifetime);

            user.LastSeen = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return new RefreshTokenLoginBody { Token = token, warning = status.Message };
        }

        public async Task<Token> UpdateUserGetNewToken(string newUsername)
        {
            if (string.IsNullOrEmpty(newUsername) ||
                await _userManager.FindByNameAsync(newUsername) is not AppUser user)
            {
                throw new Application.Exceptions.Login.UserNotFoundException("User not found", (int)StatusEnum.UserNotFound);
            }

            var token = _tokenHandler.CreateAccessToken(UsernameChangeTokenLifetime, user);
            await _accountService.UpdateRefreshTokenAsync(token.RefreshToken, user, RefreshTokenLifetime);

            return token;
        }

        public async Task<bool> IsEmailInUse(string email) =>
            await _userManager.FindByEmailAsync(email) != null;

        private async Task SendVerificationEmailAsync(AppUser user)
        {
            user.VerificationCode = _codeGenerator.Generate();

            await _userManager.UpdateAsync(user);

            _mailService.SendVerificationEmail(user.Email, "Email Confirmation", user.VerificationCode);
        }
    }
}
