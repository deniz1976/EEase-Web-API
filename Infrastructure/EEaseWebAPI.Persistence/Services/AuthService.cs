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
    /// <summary>
    /// Service responsible for handling user authentication, including login, password management, and token operations.
    /// Implements both internal and external authentication mechanisms.
    /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the AuthService with required dependencies.
        /// </summary>
        /// <param name="httpClient">HTTP client for making external API requests</param>
        /// <param name="configuration">Application configuration</param>
        /// <param name="userManager">User management service</param>
        /// <param name="tokenHandler">Token generation and handling service</param>
        /// <param name="signInManager">Sign-in management service</param>
        /// <param name="mailService">Email service for sending notifications</param>
        /// <param name="userService">User management service</param>
        /// <param name="context">Database context</param>
        /// <param name="passwordHasher">Password hashing service</param>
        /// <param name="userCacheService">User cache service</param>
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

        /// <summary>
        /// Creates a new user account using external authentication information.
        /// </summary>
        /// <param name="user">Existing user if found</param>
        /// <param name="email">User's email address</param>
        /// <param name="name">User's first name</param>
        /// <param name="info">External login information</param>
        /// <param name="accessTokenLifetime">Access token validity duration in seconds</param>
        /// <param name="surname">User's last name</param>
        /// <param name="username">Desired username</param>
        /// <param name="gender">User's gender</param>
        /// <returns>A Token object containing access and refresh tokens</returns>
        /// <exception cref="CreateUserFailedException">Thrown when external authentication fails</exception>
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

        /// <summary>
        /// Authenticates a user using their username/email and password.
        /// </summary>
        /// <param name="usernameOrEmail">User's username or email address</param>
        /// <param name="password">User's password</param>
        /// <param name="accessTokenLifetime">Access token validity duration in seconds</param>
        /// <returns>A LoginBody containing authentication tokens and account status</returns>
        /// <exception cref="UserNotFoundException">Thrown when user is not found</exception>
        /// <exception cref="UserAuthenticationException">Thrown when credentials are invalid</exception>
        /// <exception cref="EmailConfirmException">Thrown when email is not confirmed</exception>
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
                status = user.status,
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

        /// <summary>
        /// Handles checking and updating account status, including deletion of inactive accounts.
        /// </summary>
        /// <param name="user">The user whose account status needs to be checked</param>
        /// <returns>A message describing the current account status or deletion state</returns>
        /// <remarks>
        /// - Checks if account is marked for deletion
        /// - Deletes account if deletion date has passed
        /// - Removes deleted users from both database and cache
        /// - Returns appropriate status message
        /// </remarks>
        private async Task<string> HandleAccountStatusAsync(AppUser user)
        {
            if (!user.status == true && user.DeleteDate.HasValue)
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

        /// <summary>
        /// Sends a verification email to a user.
        /// </summary>
        /// <param name="user">The user to send verification to</param>
        private async Task SendVerificationEmailAsync(AppUser user)
        {
            var code = new Random().Next(100000, 999999).ToString();
            user.VerificationCode = code;
            await _userManager.UpdateAsync(user);
            _mailService.SendVerificationEmail(user.Email, "Email Confirmation", code);
        }


        /// <summary>
        /// Refreshes an authentication token using a refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token to use</param>
        /// <returns>A RefreshTokenLoginBody containing new tokens and account status</returns>
        /// <exception cref="UserNotFoundException">Thrown when user is not found or refresh token is invalid</exception>
        public async Task<RefreshTokenLoginBody> RefreshTokenLoginAsync(string refreshToken)
        {
            AppUser? user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null)
            {
                throw new Application.Exceptions.Login.UserNotFoundException("User Not Found.", (int)StatusEnum.UserNotFound);
            }

            string message = "Account is active.";

            if (user.status == false)
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

        /// <summary>
        /// Initiates the password reset process for a user.
        /// </summary>
        /// <param name="usernameOrEmail">User's username or email address</param>
        /// <returns>True if reset process was initiated successfully</returns>
        /// <exception cref="UserNotFoundException">Thrown when user is not found</exception>
        /// <exception cref="InvalidOperationException">Thrown when reset code update fails</exception>
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

        /// <summary>
        /// Generates a random reset code for password reset.
        /// </summary>
        /// <returns>A 6-digit reset code</returns>
        private string GenerateResetCode()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        /// <summary>
        /// Sends a password reset email to a user.
        /// </summary>
        /// <param name="email">The user's email address</param>
        /// <param name="resetCode">The reset code to send</param>
        private void SendResetPasswordEmail(string email, string resetCode)
        {
            _mailService.SendResetPasswordEmail(email, "Reset Password", resetCode);
        }


        /// <summary>
        /// Verifies a password reset code.
        /// </summary>
        /// <param name="code">The reset code to verify</param>
        /// <param name="usernameOrEmail">User's username or email address</param>
        /// <returns>True if code is valid</returns>
        /// <exception cref="UserNotFoundException">Thrown when user is not found</exception>
        /// <exception cref="ResetPasswordCodeNotCorrectException">Thrown when code is invalid</exception>
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

        /// <summary>
        /// Resets a user's password using a verification code.
        /// </summary>
        /// <param name="code">The verification code</param>
        /// <param name="newPassword">The new password</param>
        /// <exception cref="UserNotFoundException">Thrown when user is not found</exception>
        /// <exception cref="ResetPasswordCodeNotCorrectException">Thrown when code is invalid</exception>
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

        /// <summary>
        /// Changes a user's password.
        /// </summary>
        /// <param name="username">User's username</param>
        /// <param name="oldPassword">Current password</param>
        /// <param name="newPassword">New password</param>
        /// <returns>Success message if password was changed</returns>
        /// <exception cref="UserNotFoundException">Thrown when user is not found</exception>
        /// <exception cref="ChangePasswordFailedException">Thrown when password change fails</exception>
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


        /// <summary>
        /// Updates a user's username and generates new authentication tokens.
        /// </summary>
        /// <param name="newUsername">The new username</param>
        /// <returns>A Token object containing new access and refresh tokens</returns>
        /// <exception cref="UserNotFoundException">Thrown when user is not found</exception>
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

        /// <summary>
        /// Checks if an email address is already in use.
        /// </summary>
        /// <param name="email">The email address to check</param>
        /// <returns>True if email is already in use</returns>
        public async Task<bool> IsEmailInUse(string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }
    }
}
