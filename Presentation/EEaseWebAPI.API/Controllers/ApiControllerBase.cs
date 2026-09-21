using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EEaseWebAPI.API.Controllers
{
    public abstract class ApiControllerBase : ControllerBase
    {
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
