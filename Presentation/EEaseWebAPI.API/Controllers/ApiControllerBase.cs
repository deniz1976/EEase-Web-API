using Microsoft.AspNetCore.Mvc;
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
        /// The caller's username, taken from the token. A token without a name claim cannot
        /// reach an endpoint marked with <c>[Authorize]</c>, so this refuses rather than
        /// handing back an empty name and letting the lookup fail further in. Each endpoint
        /// used to check for that itself and answer an empty 401; the global handler answers
        /// it now, in the same shape as every other error.
        /// </summary>
        protected string CurrentUsername
        {
            get
            {
                var username = User.Identity?.Name ?? User.FindFirst(ClaimTypes.Name)?.Value;

                return string.IsNullOrWhiteSpace(username)
                    ? throw new UnauthorizedAccessException("The request carries no username.")
                    : username;
            }
        }
    }
}
