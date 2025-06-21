using EEaseWebAPI.Application.Abstractions.Services.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    /// <summary>
    /// Provides comprehensive authentication services combining both internal and external authentication methods.
    /// Implements both IExternalAuthentication and IInternalAuthentication to support multiple authentication flows.
    /// </summary>
    /// <remarks>
    /// This service:
    /// - Manages both username/password and third-party authentication
    /// - Handles token generation and management
    /// - Provides password reset and account management functionality
    /// - Supports Google Sign-In integration
    /// - Ensures secure authentication flows
    /// </remarks>
    public interface IAuthService : IExternalAuthentication, IInternalAuthentication
    {

    }
}
