using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Domain.Entities.Identity;

namespace EEaseWebAPI.Application.Abstractions.Services.Authentication
{
    public interface IAccountDeletionPolicy
    {
        Task<AccountStatus> EnforceAsync(AppUser user);
    }
}
