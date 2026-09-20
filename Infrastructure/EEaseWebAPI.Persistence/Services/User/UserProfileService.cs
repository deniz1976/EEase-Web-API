using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.UpdateUser;
using EEaseWebAPI.Application.Exceptions.UpdateUserCountry;
using EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUser;
using EEaseWebAPI.Application.Validators.User;
using EEaseWebAPI.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EEaseWebAPI.Persistence.Services.User
{
    public class UserProfileService : IUserProfileService
    {
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
                Id = user.Id,
                Username = user.UserName,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                Gender = user.Gender,
                BornDate = user.BornDate,
                Bio = user.Bio,
                Currency = user.Currency,
                PhotoPath = user.PhotoPath,
                Country = user.Country
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
                Username = targetUser.UserName,
                Name = targetUser.Name,
                Surname = targetUser.Surname,
                PhotoPath = targetUser.PhotoPath,
                Gender = targetUser.Gender,
                Country = targetUser.Country,
                Bio = targetUser.Bio,
                FriendRequestStatus = relationship.RequestStatus
            };

            if (relationship.HasFullAccess)
            {
                userInfo.Email = targetUser.Email;
                userInfo.BornDate = targetUser.BornDate;
                userInfo.Currency = targetUser.Currency;
            }

            return (userInfo, relationship.Visibility);
        }

        public async Task<(GetUserInfo userInfo, ProfileVisibilityStatus visibilityStatus)> GetUserInfoByIdAsync(
            string username, string targetUserId)
        {
            var targetUser = await _userManager.FindByIdAsync(targetUserId)
                ?? throw new Application.Exceptions.Login.UserNotFoundException(
                    "User Not Found", (int)StatusEnum.UserNotFound);

            return await GetUserInfoByNameAsync(username, targetUser.UserName!);
        }

        public async Task<bool> UpdateUser(UpdateUserCommandRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.User))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var user = await FindAsync(request.User);

            if (request.Username != null && !await IsUsernameAvailable(request.Username, user.Id))
            {
                throw new UsernameAlreadyTakenException();
            }

            Validate(request);

            user.UserName = request.Username ?? user.UserName;
            user.Name = request.Name ?? user.Name;
            user.Surname = request.Surname ?? user.Surname;
            user.Gender = request.Gender ?? user.Gender;
            user.BornDate = request.BornDate ?? user.BornDate;
            user.Bio = request.Bio ?? user.Bio;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new UpdateUserSaveException(
                    $"Failed to update the user. Errors: {Describe(result)}", (int)StatusEnum.UserUpdateFailed);
            }

            _userCacheService.UpdateUserAttributesInCache(
                user.Id, request.Username, request.Name, request.Surname, gender: request.Gender);

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

        public async Task<string?> GetUserPhotoAsync(string username) =>
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

        /// <summary>
        /// Identity treats usernames as case insensitive, so "Alice" is not free while
        /// "alice" exists; comparing the raw name let that through and the save failed later
        /// with a duplicate error nobody could act on.
        /// </summary>
        private Task<bool> IsUsernameAvailable(string username, string userId)
        {
            var normalizedUserName = username.ToUpperInvariant();

            return _userManager.Users.AllAsync(candidate =>
                candidate.NormalizedUserName != normalizedUserName || candidate.Id == userId);
        }

        /// <summary>
        /// <see cref="UpdateUserValidator"/> already answered the caller with the offending
        /// field named; this catches a call that never went through the pipeline, so the
        /// message is a sentence rather than a field map.
        /// </summary>
        private static void Validate(UpdateUserCommandRequest request)
        {
            ValidateLength(request.Name, nameof(request.Name));
            ValidateLength(request.Surname, nameof(request.Surname));

            if (request.Bio != null && request.Bio.Length > UserProfileRules.BioMaxLength)
            {
                throw new InvalidUserDataException(
                    $"Bio must be at most {UserProfileRules.BioMaxLength} characters.");
            }

            if (request.Gender != null && !UserProfileRules.IsKnownGender(request.Gender))
            {
                throw new InvalidUserDataException("Gender must be 'Male' or 'Female'.");
            }

            if (request.BornDate != null && !UserProfileRules.IsOldEnough(request.BornDate.Value))
            {
                throw new InvalidUserDataException(
                    $"User must be at least {UserProfileRules.MinimumAge} years old.");
            }
        }

        private static void ValidateLength(string? value, string field)
        {
            if (value != null &&
                (value.Length < UserProfileRules.NameMinLength ||
                 value.Length > UserProfileRules.NameMaxLength))
            {
                throw new InvalidUserDataException(
                    $"{field} must be between {UserProfileRules.NameMinLength} and " +
                    $"{UserProfileRules.NameMaxLength} characters.");
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
