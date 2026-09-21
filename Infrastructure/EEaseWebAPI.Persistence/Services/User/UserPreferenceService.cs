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

        public async Task<AppUser> GetUserWithPreferencesAsync(string username, CancellationToken cancellationToken = default)
        {
            // Identity looks users up by the normalised name, so comparing the raw one here
            // would miss a caller that spelled it with different capitals.
            var normalizedUserName = username.ToUpperInvariant();

            var user = await _context.Users
                .Include(appUser => appUser.UserPersonalization)
                .Include(appUser => appUser.FoodPreferences)
                .Include(appUser => appUser.AccommodationPreferences)
                .FirstOrDefaultAsync(appUser => appUser.NormalizedUserName == normalizedUserName, cancellationToken);

            return user ?? throw new Application.Exceptions.Login.UserNotFoundException("User not found", (int)StatusEnum.UserNotFound);
        }

        public async Task SetFromMessageAsync(
            string username, string message, CancellationToken cancellationToken = default)
        {
            var user = await GetUserWithPreferencesAsync(username);

            EnsureNoPreferences(user);

            var (accommodation, food, personalization) =
                await _geminiAIService.GetUserPreferencesFromMessage(message, cancellationToken);

            if (!HasAnyPreference(accommodation) && !HasAnyPreference(food) && !HasAnyPreference(personalization))
            {
                throw new UpdateUserSaveException(
                    "Preferences could not be extracted from your message. Please provide more specific details about your preferences.",
                    (int)StatusEnum.PreferencesUpdateFailed);
            }

            await SaveNewPreferencesAsync(user.Id, accommodation, food, personalization, cancellationToken);
        }

        public async Task SetFromTopicsAsync(string username, IReadOnlyList<string> topics, CancellationToken cancellationToken = default)
        {
            var user = await GetUserWithPreferencesAsync(username);

            EnsureNoPreferences(user);

            var accommodation = new UserAccommodationPreferences { UserId = user.Id };
            var food = new UserFoodPreferences { UserId = user.Id };
            var personalization = new UserPersonalization { UserId = user.Id };

            foreach (var topic in topics ?? Array.Empty<string>())
            {
                if (!TravelPreferenceGroups.TryGetPreferenceNames(topic, out var preferenceNames))
                {
                    // Ignoring it answered "preferences saved" to a caller whose topics were
                    // all misspelled and whose preferences were therefore all empty.
                    throw new UpdateUserSaveException(
                        $"'{topic}' is not one of the topics. Ask for the topic list first.",
                        (int)StatusEnum.PreferencesUpdateFailed);
                }

                Score(preferenceNames, accommodation, food, personalization);
            }

            await SaveNewPreferencesAsync(user.Id, accommodation, food, personalization, cancellationToken);
        }

        public async Task ResetAsync(string username, CancellationToken cancellationToken = default)
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

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<GetUserPreferenceDescriptionsBody> GetDescriptionsAsync(string username, CancellationToken cancellationToken = default)
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
            string viewerUsername, string targetUsername,
            CancellationToken cancellationToken = default)
        {
            if (viewerUsername != targetUsername &&
                !await _friendshipService.AreFriendsAsync(viewerUsername, targetUsername, cancellationToken))
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
            UserPersonalization personalization,
            CancellationToken cancellationToken)
        {
            accommodation.UserId = userId;
            food.UserId = userId;
            personalization.UserId = userId;

            // The params overload would take the token for an entity, so the rows go in as
            // an array.
            await _context.AddRangeAsync(
                new object[] { accommodation, food, personalization }, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
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

        private static void Score(
            IEnumerable<string> propertyNames,
            UserAccommodationPreferences accommodation,
            UserFoodPreferences food,
            UserPersonalization personalization)
        {
            foreach (var propertyName in propertyNames)
            {
                if (!TryScore(accommodation, propertyName) &&
                    !TryScore(food, propertyName) &&
                    !TryScore(personalization, propertyName))
                {
                    throw new InvalidOperationException(
                        $"No preference row has a '{propertyName}' column.");
                }
            }
        }

        private static bool TryScore(object preferences, string propertyName)
        {
            var property = preferences.GetType().GetProperty(propertyName);

            if (property?.PropertyType != typeof(int?))
            {
                return false;
            }

            property.SetValue(preferences, SelectedTopicScore);

            return true;
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
