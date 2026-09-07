using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.UpdateUser;
using EEaseWebAPI.Application.Exceptions.UpdateUserCountry;
using EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUser;
using EEaseWebAPI.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EEaseWebAPI.Persistence.Services.User
{
    public class UserProfileService : IUserProfileService
    {
        private const int NameMinLength = 2;
        private const int NameMaxLength = 16;
        private const int BioMaxLength = 80;

        private static readonly string[] Genders = { "Male", "Female" };

        private readonly UserManager<AppUser> _userManager;
        private readonly ICityService _cityService;
        private readonly ICurrencyService _currencyService;
        private readonly IUserCacheService _userCacheService;
        private readonly IFriendshipService _friendshipService;

        public UserProfileService(
            UserManager<AppUser> userManager,
            ICityService cityService,
            ICurrencyService currencyService,
            IUserCacheService userCacheService,
            IFriendshipService friendshipService)
        {
            _userManager = userManager;
            _cityService = cityService;
            _currencyService = currencyService;
            _userCacheService = userCacheService;
            _friendshipService = friendshipService;
        }

        public async Task<GetUserInfo> GetUserInfoQuery(string username)
        {
            var user = await FindAsync(username);

            return new GetUserInfo
            {
                id = user.Id,
                username = user.UserName,
                name = user.Name,
                surname = user.Surname,
                email = user.Email,
                gender = user.Gender,
                borndate = user.BornDate,
                bio = user.Bio,
                currency = user.Currency,
                photoPath = user.PhotoPath,
                country = user.Country
            };
        }

        public async Task<(GetUserInfo userInfo, ProfileVisibilityStatus visibilityStatus)> GetUserInfoByNameAsync(
            string username, string targetUsername)
        {
            var targetUser = await FindAsync(targetUsername);
            var relationship = await _friendshipService.GetRelationshipAsync(username, targetUsername);

            var userInfo = new GetUserInfo
            {
                Id = targetUser.Id,
                username = targetUser.UserName,
                name = targetUser.Name,
                surname = targetUser.Surname,
                photoPath = targetUser.PhotoPath,
                gender = targetUser.Gender,
                country = targetUser.Country,
                bio = targetUser.Bio,
                friendRequestStatus = relationship.RequestStatus
            };

            if (relationship.HasFullAccess)
            {
                userInfo.email = targetUser.Email;
                userInfo.borndate = targetUser.BornDate;
                userInfo.currency = targetUser.Currency;
            }

            return (userInfo, relationship.Visibility);
        }

        public async Task<(GetUserInfo userInfo, ProfileVisibilityStatus visibilityStatus)> GetUserInfoByIdAsync(
            string username, string targetUserId)
        {
            var targetUser = await _userManager.FindByIdAsync(targetUserId)
                ?? throw new Application.Exceptions.Login.UserNotFoundException(
                    "User Not Found", (int)StatusEnum.UserNotFound);

            return await GetUserInfoByNameAsync(username, targetUser.UserName);
        }

        public async Task<bool> UpdateUser(UpdateUserCommandRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.user))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var user = await FindAsync(request.user);

            if (request.Username != null && !await IsUsernameAvailable(request.Username, user.Id))
            {
                throw new ArgumentException("Username must be unique.");
            }

            ValidateLength(request.Name, nameof(request.Name), NameMinLength, NameMaxLength);
            ValidateLength(request.Surname, nameof(request.Surname), NameMinLength, NameMaxLength);

            if (request.bio != null && request.bio.Length > BioMaxLength)
            {
                throw new ArgumentException($"Bio must be at most {BioMaxLength} characters.");
            }

            if (request.Gender != null && !Genders.Contains(request.Gender))
            {
                throw new ArgumentException("Gender must be 'Male' or 'Female'.");
            }

            if (request.BornDate != null &&
                UserRegistrationService.AgeOn(request.BornDate.Value) < UserRegistrationService.MinimumAge)
            {
                throw new ArgumentException(
                    $"User must be at least {UserRegistrationService.MinimumAge} years old.");
            }

            user.UserName = request.Username ?? user.UserName;
            user.Name = request.Name ?? user.Name;
            user.Surname = request.Surname ?? user.Surname;
            user.Gender = request.Gender ?? user.Gender;
            user.BornDate = request.BornDate ?? user.BornDate;
            user.Bio = request.bio ?? user.Bio;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new UpdateUserSaveException(
                    $"Failed to update the user. Errors: {Describe(result)}", (int)StatusEnum.UserUpdateFailed);
            }

            _userCacheService.UpdateUserAttributesInCache(
                user.Id, request.Username, request.Name, request.Surname);

            return true;
        }

        public async Task<bool> UpdateUserCountry(string username, string country)
        {
            var availableCountries = await _cityService.GetAllCountries();

            if (!availableCountries.Contains(country, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidCountryException($"Country '{country}' is not in the available countries list.");
            }

            var user = await FindAsync(username);
            user.Country = country;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new UpdateUserSaveException(
                    $"Failed to update the user country. Errors: {Describe(result)}",
                    (int)StatusEnum.UserUpdateFailed);
            }

            return true;
        }

        public async Task<bool> UpdateUserCurrency(string username, string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
            {
                throw new ArgumentException("Currency code cannot be null or empty.", nameof(currencyCode));
            }

            var user = await FindAsync(username);
            var currencies = await _currencyService.GetCurrenciesAsync();

            var isKnown = currencies.Any(currency =>
                string.Equals(currency.AlphabeticCode, currencyCode, StringComparison.OrdinalIgnoreCase));

            if (!isKnown)
            {
                throw new UpdateUserSaveException(
                    $"Currency '{currencyCode}' is not a known currency code.", (int)StatusEnum.UserUpdateFailed);
            }

            user.Currency = currencyCode;

            return (await _userManager.UpdateAsync(user)).Succeeded;
        }

        public async Task<string> GetUserCurrencyAsync(string username)
        {
            var user = await FindAsync(username);

            return user.Currency
                ?? throw new UpdateUserSaveException(
                    "The user has no currency set.", (int)StatusEnum.UserUpdateFailed);
        }

        public async Task<string> GetUserPhotoAsync(string username) =>
            (await FindAsync(username)).PhotoPath;

        public async Task<bool> SetUserPhoto(string username, string photoPath)
        {
            var user = await FindAsync(username);
            user.PhotoPath = photoPath;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new UpdateUserSaveException(
                    $"Failed to update user photo. Errors: {Describe(result)}", (int)StatusEnum.UserUpdateFailed);
            }

            _userCacheService.UpdateUserAttributesInCache(user.Id, photoUrl: photoPath);

            return true;
        }

        private Task<bool> IsUsernameAvailable(string username, string userId) =>
            _userManager.Users.AllAsync(candidate =>
                candidate.UserName != username || candidate.Id == userId);

        private static void ValidateLength(string? value, string field, int minimum, int maximum)
        {
            if (value != null && (value.Length < minimum || value.Length > maximum))
            {
                throw new ArgumentException($"{field} must be between {minimum} and {maximum} characters.");
            }
        }

        private async Task<AppUser> FindAsync(string username) =>
            await _userManager.FindByNameAsync(username)
            ?? throw new Application.Exceptions.Login.UserNotFoundException(
                "User not found", (int)StatusEnum.UserNotFound);

        private static string Describe(IdentityResult result) =>
            string.Join("; ", result.Errors.Select(error => error.Description));
    }
}
