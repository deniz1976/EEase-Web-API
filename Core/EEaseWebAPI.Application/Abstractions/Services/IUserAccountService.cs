using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.MapEntities.StatusCheck;
using EEaseWebAPI.Domain.Entities.Identity;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IUserAccountService
    {
        Task UpdateRefreshTokenAsync(string refreshToken, AppUser user, TimeSpan lifetime);

        Task<DeleteRequestOutcome> RequestDeletionAsync(string username);

        Task<string> ConfirmDeletionAsync(string username, string code);

        Task<StatusCheckBody> StatusCheck(string username);
    }
}
