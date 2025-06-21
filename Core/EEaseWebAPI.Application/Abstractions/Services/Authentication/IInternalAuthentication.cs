using EEaseWebAPI.Application.MapEntities.Login;
using EEaseWebAPI.Application.MapEntities.RefreshTokenLogin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Abstractions.Services.Authentication
{
    /// <summary>
    /// Manages internal authentication operations including login, password management, and token handling.
    /// Provides comprehensive authentication functionality for users using username/password credentials.
    /// </summary>
    public interface IInternalAuthentication
    {
        /// <summary>
        /// Authenticates a user using username/email and password.
        /// </summary>
        /// <param name="usernameOrEmail">User's username or email address</param>
        /// <param name="password">User's password</param>
        /// <param name="accessTokenLifetime">Duration in minutes for the access token validity</param>
        /// <returns>Login response containing authentication tokens and user information</returns>
        Task<LoginBody> LoginAsync(string usernameOrEmail, string password, int accessTokenLifetime);

        /// <summary>
        /// Refreshes an authentication session using a refresh token.
        /// </summary>
        /// <param name="refreshToken">Valid refresh token from a previous authentication</param>
        /// <returns>New authentication tokens and session information</returns>
        Task<RefreshTokenLoginBody> RefreshTokenLoginAsync(string refreshToken);

        /// <summary>
        /// Initiates the password reset process for a user.
        /// </summary>
        /// <param name="usernameOrEmail">Username or email of the account to reset</param>
        /// <returns>True if reset process initiated successfully, false otherwise</returns>
        Task<bool> ResetPassword(string usernameOrEmail);

        /// <summary>
        /// Validates a password reset code.
        /// </summary>
        /// <param name="code">Reset code to validate</param>
        /// <param name="usernameOrEmail">Username or email of the account</param>
        /// <returns>True if code is valid, false otherwise</returns>
        Task<bool> ResetPasswordCodeCheck(string code, string usernameOrEmail);

        /// <summary>
        /// Completes the password reset process with a new password.
        /// </summary>
        /// <param name="code">Valid reset code</param>
        /// <param name="newPassword">New password to set</param>
        Task ResetPasswordWithCode(string code, string newPassword);

        /// <summary>
        /// Changes a user's password after verifying the old password.
        /// </summary>
        /// <param name="username">Username of the account</param>
        /// <param name="oldPassword">Current password</param>
        /// <param name="newPassword">New password to set</param>
        /// <returns>Success message if password changed successfully</returns>
        Task<string> ChangePassword(string username, string oldPassword, string newPassword);

        /// <summary>
        /// Updates a user's username and generates new authentication tokens.
        /// </summary>
        /// <param name="newUsername">New username to set</param>
        /// <returns>New authentication tokens for the updated user</returns>
        Task<DTOs.Token> UpdateUserGetNewToken(string newUsername);

        /// <summary>
        /// Checks if an email address is already registered.
        /// </summary>
        /// <param name="email">Email address to check</param>
        /// <returns>True if email is in use, false otherwise</returns>
        Task<bool> IsEmailInUse(string email);
    }
}
