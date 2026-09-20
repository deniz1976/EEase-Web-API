using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.MapEntities.StatusCheck;
using EEaseWebAPI.Domain.Entities.Identity;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IUserAccountService
    {
        Task UpdateRefreshTokenAsync(string refreshToken, AppUser user, TimeSpan lifetime, CancellationToken cancellationToken = default);

        Task<DeleteRequestOutcome> RequestDeletionAsync(string username, CancellationToken cancellationToken = default);

        Task<string> ConfirmDeletionAsync(string username, string code, CancellationToken cancellationToken = default);

        Task<StatusCheckBody> StatusCheck(string username, CancellationToken cancellationToken = default);
    }
}
