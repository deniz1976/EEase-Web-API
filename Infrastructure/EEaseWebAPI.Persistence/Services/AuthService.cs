using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Abstractions.Token;
using EEaseWebAPI.Application.DTOs;
using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Login;
using EEaseWebAPI.Application.Exceptions.ResetPassword;
using EEaseWebAPI.Application.Exceptions.ChangePassword;
using EEaseWebAPI.Application.MapEntities.Login;
using EEaseWebAPI.Application.MapEntities.RefreshTokenLogin;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EEaseWebAPI.Application.Exceptions.CreateUser;

namespace EEaseWebAPI.Persistence.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly UserManager<Domain.Entities.Identity.AppUser> _userManager;
        private readonly ITokenHandler _tokenHandler;
        private readonly SignInManager<Domain.Entities.Identity.AppUser> _signInManager;
        private readonly IMailService _mailService;
        private readonly IUserService _userService;
        private readonly EEaseAPIDbContext _context;
        private readonly PasswordHasher<string> _passwordHasher;
        private readonly IUserCacheService _userCacheService;

        public AuthService(HttpClient httpClient, IConfiguration configuration, UserManager<Domain.Entities.Identity.AppUser> userManager, ITokenHandler tokenHandler, SignInManager<Domain.Entities.Identity.AppUser> signInManager, IMailService mailService, IUserService userService, EEaseAPIDbContext context, PasswordHasher<string> passwordHasher, IUserCacheService userCacheService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _userManager = userManager;
            _tokenHandler = tokenHandler;
            _signInManager = signInManager;
            _mailService = mailService;
            _userService = userService;
            _context = context; 
            _passwordHasher = passwordHasher;
            _userCacheService = userCacheService;
        }

        public async Task<Token> CreateUserExternalAsync(AppUser user, string email, string name, UserLoginInfo info, int accessTokenLifetime, string surname, string username, string gender)
        {
            bool result = user != null;
            if(user == null) 
            {
                user = await _userManager.FindByEmailAsync(email);
                if(user == null) 
                {
                    user = new() 
                    {
                        Id=Guid.NewGuid().ToString(),
                        Email=email,
                        Name=name,
                        UserName=username,
                        Gender=gender,
                        Surname=surname,
                        
                    };

                    var identityResult = await _userManager.CreateAsync(user);
                    result = identityResult.Succeeded;
                }
            }

            if (result)
            {
                await _userManager.AddLoginAsync(user,info);
                Token token = _tokenHandler.CreateAccessToken(accessTokenLifetime,user);
                return token;
            }
            throw new CreateUserFailedException("Invalid external authentication.",(int)StatusEnum.CreateUserFailed);
        }
        
        public Task<Token> GoogleLoginAsync(string idToken, int accessTokenLifeTime)
        {
            throw new NotImplementedException();
        }

        public async Task<LoginBody> LoginAsync(string usernameOrEmail, string password, int accessTokenLifetime)
        {
            var user = await _userManager.FindByNameAsync(usernameOrEmail)
                        ?? await _userManager.FindByEmailAsync(usernameOrEmail);

            if (user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User not found", (int)StatusEnum.UserNotFound);

            string message = await HandleAccountStatusAsync(user);

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            if (!result.Succeeded)
                throw new UserAuthenticationException("Username or password is incorrect", (int)StatusEnum.InvalidCredentials);

            if (!user.EmailConfirmed)
            {
                await SendVerificationEmailAsync(user);
                throw new Application.Exceptions.EmailConfirmException("Email confirmation required, new code sent to email.", (int)StatusEnum.EmailNotConfirmed);
            }

            var token = _tokenHandler.CreateAccessToken(accessTokenLifetime, user);
            await _userService.UpdateRefreshTokenAsync(token.RefreshToken, user, token.Expiration, 15);

            var x = user.LastSeen;
            user.LastSeen = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var userInfo = new UserInfo()
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
                lastSeen = x,
            };

            return new LoginBody
            {
                token = token,
                warning = message,
                userInfo = userInfo
                
            };
        }

        private async Task<string> HandleAccountStatusAsync(AppUser user)
        {
            if (!user.Status == true && user.DeleteDate.HasValue)
            {
                var daysUntilDeletion = (user.DeleteDate.Value.Date - DateTime.Now.Date).Days;

                if (daysUntilDeletion <= 0)
                {
                    await _userManager.DeleteAsync(user);
                    await _context.SaveChangesAsync();
                    _userCacheService.RemoveUserFromCache(user.Id);
                    return "Account deleted";
                }

                return $"Account will be deleted in {daysUntilDeletion} days";
            }

            return "Account is active.";
        }

        private async Task SendVerificationEmailAsync(AppUser user)
        {
            var code = new Random().Next(100000, 999999).ToString();
            user.VerificationCode = code;
            await _userManager.UpdateAsync(user);
            _mailService.SendVerificationEmail(user.Email, "Email Confirmation", code);
        }

        public async Task<RefreshTokenLoginBody> RefreshTokenLoginAsync(string refreshToken)
        {
            AppUser? user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null)
            {
                throw new Application.Exceptions.Login.UserNotFoundException("User Not Found.", (int)StatusEnum.UserNotFound);
            }

            string message = "Account is active.";

            if (user.Status == false)
            {
                if (user.DeleteDate.HasValue)
                {
                    double daysSinceDeactivation = (DateTime.UtcNow - user.DeleteDate.Value).TotalDays;

                    if (daysSinceDeactivation >= 7)
                    {
                        await _userManager.DeleteAsync(user);
                        await _context.SaveChangesAsync();

                        message = "Account deleted";
                        return new RefreshTokenLoginBody
                        {
                            Token = null,
                            warning = message
                        };
                    }
                    else
                    {
                        int daysUntilDeletion = (int)Math.Ceiling(7 - daysSinceDeactivation);
                        message = $"Account will be deleted in {daysUntilDeletion} day(s).";
                    }
                }
            }

            if (user.RefreshTokenEndDate > DateTime.UtcNow)
            {
                Token token = _tokenHandler.CreateAccessToken(15 * 60, user);
                await _userService.UpdateRefreshTokenAsync(token.RefreshToken, user, token.Expiration, 300);

                user.LastSeen = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                return new RefreshTokenLoginBody
                {
                    Token =new() 
                    {
                        AccessToken = token.AccessToken,
                        RefreshToken = token.RefreshToken,
                        Expiration = token.Expiration
                    },
                    warning = message
                };
            }

            throw new Application.Exceptions.Login.UserNotFoundException("Refresh token has expired or is invalid.", (int)StatusEnum.RefreshTokenExpired);
        }

        public async Task<bool> ResetPassword(string usernameOrEmail)
        {
            var user = await _userManager.FindByEmailAsync(usernameOrEmail)
                       ?? await _userManager.FindByNameAsync(usernameOrEmail);

            if (user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User not found", (int)StatusEnum.UserNotFound);

            var resetCode = GenerateResetCode();
            user.ResetPasswordCode = resetCode;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                throw new InvalidOperationException("Failed to update user reset password code.");

            SendResetPasswordEmail(user.Email, resetCode);

            return true;
        }

        private string GenerateResetCode()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        private void SendResetPasswordEmail(string email, string resetCode)
        {
            _mailService.SendResetPasswordEmail(email, "Reset Password", resetCode);
        }

        public async Task<bool> ResetPasswordCodeCheck(string code, string usernameOrEmail)
        {
            var user = await _userManager.FindByEmailAsync(usernameOrEmail)
                       ?? await _userManager.FindByNameAsync(usernameOrEmail);

            if (user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User not found", (int)StatusEnum.UserNotFound);

            if (user.ResetPasswordCode == code)
                return true;

            throw new ResetPasswordCodeNotCorrectException("Reset password code is not correct", (int)StatusEnum.InvalidResetPasswordCode);
        }

        public async Task ResetPasswordWithCode(string code, string newPassword)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.ResetPasswordCode == code);

            if (user == null)
                throw new ResetPasswordCodeNotCorrectException("Reset password code is not correct", (int)StatusEnum.InvalidResetPasswordCode);

            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(null, user.PasswordHash, newPassword);
            if (passwordVerificationResult == PasswordVerificationResult.Success)
                throw new PasswordChangeException("Old password and new password are the same.", (int)StatusEnum.PasswordChangeFailed);

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to reset password: {errors}");
            }

            user.ResetPasswordCode = null;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var updateErrors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                throw new Exception($"Failed to clear reset password code: {updateErrors}");
            }
        }

        public async Task<string> ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (oldPassword == newPassword)
                throw new SamePasswordsException("Old password and new password are the same.", (int)StatusEnum.PasswordChangeFailed);

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User not found.", (int)StatusEnum.UserNotFound);

            var isOldPasswordCorrect = await _userManager.CheckPasswordAsync(user, oldPassword);
            if (!isOldPasswordCorrect)
                throw new InvalidPasswordException("The old password is incorrect.", (int)StatusEnum.InvalidPassword);

            if (string.IsNullOrWhiteSpace(newPassword))
                return "The old password is correct. Please provide a new password.";

            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new PasswordChangeException($"Failed to change the password. Errors: {errors}", (int)StatusEnum.PasswordChangeFailed);
            }

            return "Password changed successfully.";
        }

        public async Task<Token> UpdateUserGetNewToken(string newUsername) 
        {
            if (string.IsNullOrEmpty(newUsername) ||
                await _userManager.FindByNameAsync(newUsername) is not AppUser appUser)
            {
                throw new Application.Exceptions.Login.UserNotFoundException("User not found", (int)StatusEnum.UserNotFound);
            }

            Token token = _tokenHandler.CreateAccessToken(24 * 60 * 60, appUser);
            await _userService.UpdateRefreshTokenAsync(token.RefreshToken, appUser, token.Expiration, 15);
            return token;

        }

        public async Task<bool> IsEmailInUse(string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }
    }
}
