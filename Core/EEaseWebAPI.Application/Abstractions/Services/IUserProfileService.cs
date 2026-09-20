using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUser;
using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IUserProfileService
    {
        Task<GetUserInfo> GetUserInfoQuery(string username, CancellationToken cancellationToken = default);

        Task<(GetUserInfo userInfo, ProfileVisibilityStatus visibilityStatus)> GetUserInfoByNameAsync(
            string username, string targetUsername,
            CancellationToken cancellationToken = default);

        Task<(GetUserInfo userInfo, ProfileVisibilityStatus visibilityStatus)> GetUserInfoByIdAsync(
            string username, string targetUserId,
            CancellationToken cancellationToken = default);

        Task<bool> UpdateUser(UpdateUserCommandRequest request, CancellationToken cancellationToken = default);

        Task<bool> UpdateUserCountry(string username, string country, CancellationToken cancellationToken = default);

        Task<bool> UpdateUserCurrency(string username, string currencyCode, CancellationToken cancellationToken = default);

        Task<string> GetUserCurrencyAsync(string username, CancellationToken cancellationToken = default);

        Task<string?> GetUserPhotoAsync(string username, CancellationToken cancellationToken = default);

        Task<bool> SetUserPhoto(string username, string photoPath, CancellationToken cancellationToken = default);
    }
}
