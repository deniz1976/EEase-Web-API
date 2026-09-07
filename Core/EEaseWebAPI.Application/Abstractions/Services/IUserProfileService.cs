using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUser;
using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IUserProfileService
    {
        Task<GetUserInfo> GetUserInfoQuery(string username);

        Task<(GetUserInfo userInfo, ProfileVisibilityStatus visibilityStatus)> GetUserInfoByNameAsync(
            string username, string targetUsername);

        Task<(GetUserInfo userInfo, ProfileVisibilityStatus visibilityStatus)> GetUserInfoByIdAsync(
            string username, string targetUserId);

        Task<bool> UpdateUser(UpdateUserCommandRequest request);

        Task<bool> UpdateUserCountry(string username, string country);

        Task<bool> UpdateUserCurrency(string username, string currencyCode);

        Task<string> GetUserCurrencyAsync(string username);

        Task<string> GetUserPhotoAsync(string username);

        Task<bool> SetUserPhoto(string username, string photoPath);
    }
}
