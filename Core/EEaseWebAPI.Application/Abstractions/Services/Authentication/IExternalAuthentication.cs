using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Abstractions.Services.Authentication
{
    /// <summary>
    /// Manages external authentication operations with third-party providers.
    /// Handles authentication flows for external identity providers such as Google.
    /// </summary>
    public interface IExternalAuthentication
    {
        /// <summary>
        /// Authenticates a user using their Google ID token.
        /// </summary>
        /// <param name="idToken">Google ID token obtained from Google Sign-In</param>
        /// <param name="accessTokenLifeTime">Duration in minutes for the access token validity</param>
        /// <returns>Authentication tokens for the application</returns>
        /// <remarks>
        /// - Validates the Google ID token
        /// - Creates or updates user account if necessary
        /// - Generates application-specific authentication tokens
        /// - Manages user profile synchronization
        /// </remarks>
        Task<DTOs.Token> GoogleLoginAsync(string idToken, int accessTokenLifeTime);
    }
}
