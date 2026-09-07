using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Login;
using EEaseWebAPI.Application.Exceptions.ResetUserPreferences;
using EEaseWebAPI.Application.Exceptions.UpdateUser;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetAllTopics;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetUserPreferenceDescriptions;
using EEaseWebAPI.Application.MapEntities.PreferenceGroups;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Enums;
using EEaseWebAPI.Domain.Extensions;
using EEaseWebAPI.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EEaseWebAPI.Persistence.Services.User
{
    public class UserPreferenceService : IUserPreferenceService
    {
        private const int SelectedTopicScore = 60;
        private const int DescriptionThreshold = 45;

        private readonly EEaseAPIDbContext _context;
        private readonly IGeminiAIService _geminiAIService;
        private readonly IFriendshipService _friendshipService;

        public UserPreferenceService(
            EEaseAPIDbContext context,
            IGeminiAIService geminiAIService,
            IFriendshipService friendshipService)
        {
            _context = context;
            _geminiAIService = geminiAIService;
            _friendshipService = friendshipService;
        }

        public async Task<AppUser> GetUserWithPreferencesAsync(string username)
        {
            var user = await _context.Users
                .Include(appUser => appUser.UserPersonalization)
                .Include(appUser => appUser.FoodPreferences)
                .Include(appUser => appUser.AccommodationPreferences)
                .FirstOrDefaultAsync(appUser => appUser.UserName == username);

            return user ?? throw new Application.Exceptions.Login.UserNotFoundException("User not found", (int)StatusEnum.UserNotFound);
        }

        public async Task SetFromMessageAsync(string username, string message)
        {
            var user = await GetUserWithPreferencesAsync(username);

            EnsureNoPreferences(user);

            var (accommodation, food, personalization) =
                await _geminiAIService.GetUserPreferencesFromMessage(message);

            if (!HasAnyPreference(accommodation) && !HasAnyPreference(food) && !HasAnyPreference(personalization))
            {
                throw new UpdateUserSaveException(
                    "Preferences could not be extracted from your message. Please provide more specific details about your preferences.",
                    (int)StatusEnum.PreferencesUpdateFailed);
            }

            await SaveNewPreferencesAsync(user.Id, accommodation, food, personalization);
        }

        public async Task SetFromTopicsAsync(string username, IReadOnlyList<string> topics)
        {
            var user = await GetUserWithPreferencesAsync(username);

            EnsureNoPreferences(user);

            var accommodation = new UserAccommodationPreferences { UserId = user.Id };
            var food = new UserFoodPreferences { UserId = user.Id };
            var personalization = new UserPersonalization { UserId = user.Id };

            foreach (var topic in topics ?? Array.Empty<string>())
            {
                if (TravelPreferenceGroups.AccommodationGroups.Groups.TryGetValue(topic, out var accommodationNames))
                {
                    Score(accommodation, accommodationNames);
                }
                else if (TravelPreferenceGroups.FoodGroups.Groups.TryGetValue(topic, out var foodNames))
                {
                    Score(food, foodNames);
                }
                else if (TravelPreferenceGroups.TravelGroups.Groups.TryGetValue(topic, out var travelNames))
                {
                    Score(personalization, travelNames);
                }
            }

            await SaveNewPreferencesAsync(user.Id, accommodation, food, personalization);
        }

        public async Task ResetAsync(string username)
        {
            var user = await GetUserWithPreferencesAsync(username);

            if (user.AccommodationPreferences == null &&
                user.FoodPreferences == null &&
                user.UserPersonalization == null)
            {
                throw new ResetUserPreferencesFailedException("User has no preferences to reset");
            }

            if (user.AccommodationPreferences != null)
                _context.Remove(user.AccommodationPreferences);

            if (user.FoodPreferences != null)
                _context.Remove(user.FoodPreferences);

            if (user.UserPersonalization != null)
                _context.Remove(user.UserPersonalization);

            await _context.SaveChangesAsync();
        }

        public async Task<GetUserPreferenceDescriptionsBody> GetDescriptionsAsync(string username)
        {
            var user = await GetUserWithPreferencesAsync(username);

            var response = new GetUserPreferenceDescriptionsBody();

            Describe<PersonalizationTypes>(user.UserPersonalization, response.PersonalizationPreferences);
            Describe<FoodPreferenceTypes>(user.FoodPreferences, response.FoodPreferences);
            Describe<AccommodationPreferenceTypes>(user.AccommodationPreferences, response.AccommodationPreferences);

            if (response.PersonalizationPreferences.Count == 0 &&
                response.FoodPreferences.Count == 0 &&
                response.AccommodationPreferences.Count == 0)
            {
                throw new BaseException(
                    $"No preferences found with value greater than {DescriptionThreshold}",
                    (int)StatusEnum.PreferenceDescriptionsRetrievalFailed);
            }

            return response;
        }

        public async Task<GetUserPreferenceDescriptionsBody?> GetDescriptionsForViewerAsync(
            string viewerUsername, string targetUsername)
        {
            if (viewerUsername != targetUsername &&
                !await _friendshipService.AreFriendsAsync(viewerUsername, targetUsername))
            {
                return null;
            }

            try
            {
                return await GetDescriptionsAsync(targetUsername);
            }
            catch (BaseException)
            {
                return null;
            }
        }

        public GetAllTopicsQueryResponseBody GetAllTopics() =>
            new()
            {
                AccommodationTopics = [.. TravelPreferenceGroups.AccommodationGroups.Groups.Keys],
                FoodTopics = [.. TravelPreferenceGroups.FoodGroups.Groups.Keys],
                TravelTopics = [.. TravelPreferenceGroups.TravelGroups.Groups.Keys]
            };

        private async Task SaveNewPreferencesAsync(
            string userId,
            UserAccommodationPreferences accommodation,
            UserFoodPreferences food,
            UserPersonalization personalization)
        {
            accommodation.UserId = userId;
            food.UserId = userId;
            personalization.UserId = userId;

            await _context.AddRangeAsync(accommodation, food, personalization);
            await _context.SaveChangesAsync();
        }

        private static void EnsureNoPreferences(AppUser user)
        {
            if (user.AccommodationPreferences != null ||
                user.FoodPreferences != null ||
                user.UserPersonalization != null)
            {
                throw new UserAlreadyHasPreferencesException(
                    "User already has preferences set. Please use reset preferences first.");
            }
        }

        private static void Score(object preferences, IEnumerable<string> propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                preferences.GetType().GetProperty(propertyName)?.SetValue(preferences, SelectedTopicScore);
            }
        }

        private static void Describe<TPreferenceType>(object? preferences, List<PreferenceDetail> target)
            where TPreferenceType : struct, Enum
        {
            if (preferences == null)
            {
                return;
            }

            foreach (var preferenceType in Enum.GetValues<TPreferenceType>())
            {
                var property = preferences.GetType().GetProperty($"{preferenceType}Preference");

                if (property?.GetValue(preferences) is not int value || value <= DescriptionThreshold)
                {
                    continue;
                }

                target.Add(new PreferenceDetail
                {
                    Description = preferenceType.GetDescription(),
                    Value = value
                });
            }
        }

        private static bool HasAnyPreference(object? preferences) =>
            preferences != null &&
            preferences.GetType().GetProperties()
                .Where(IsPreferenceProperty)
                .Any(property => property.GetValue(preferences) != null);

        private static bool IsPreferenceProperty(PropertyInfo property) =>
            property.PropertyType == typeof(int?) &&
            property.Name is not ("UserId" or "Id" or "User");
    }
}
