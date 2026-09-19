using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace EEaseWebAPI.API.Controllers
{
    /// <summary>
    /// Shared base for the API controllers. It only answers one question every
    /// authenticated endpoint asks: who is calling?
    /// </summary>
    public abstract class ApiControllerBase : ControllerBase
    {
        /// <summary>
        /// The caller's username taken from the token. Empty when the request carries no
        /// identity, which cannot happen on an endpoint marked with <c>[Authorize]</c>.
        /// </summary>
        protected string CurrentUsername =>
            User.Identity?.Name ?? User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

        /// <summary>
        /// Use this where the endpoint answers <c>401</c> itself instead of relying on the
        /// authentication middleware.
        /// </summary>
        protected bool TryGetCurrentUsername([NotNullWhen(true)] out string? username)
        {
            username = CurrentUsername;

            if (string.IsNullOrEmpty(username))
            {
                username = null;

                return false;
            }

            return true;
        }
    }
}
