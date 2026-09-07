using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Common;
using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Domain.Entities.Identity;
using System.Reflection;

namespace EEaseWebAPI.Persistence.Services.Route
{
    public sealed class PreferenceProfileBuilder : IPreferenceProfileBuilder
    {
        private const double GroupMeanWeight = 0.6d;
        private const double GroupMaxWeight = 0.4d;
        private const double DominantMultiplier = 2d;
        private const int VeganBoostMultiplier = 2;

        private static readonly HashSet<string> DominantAccommodation = new()
        {
            "PetFriendlyPreference",
            "FamilyFriendlyPreference",
            "AdultsOnlyPreference",
            "EcoFriendlyPreference"
        };

        private static readonly HashSet<string> DominantFood = new()
        {
            "VeganPreference",
            "VegetarianPreference",
            "HalalPreference",
            "KosherPreference",
            "GlutenFreePreference",
            "DairyFreePreference",
            "NutFreePreference",
            "AllergiesPreference"
        };

        private static readonly HashSet<string> DominantPersonalization = new()
        {
            "AdventurePreference",
            "FamilyTravelPreference",
            "LuxuryPreference",
            "BudgetPreference"
        };

        private static readonly Dictionary<string, (int Threshold, int Priority)> MandatoryFood = new()
        {
            ["VeganPreference"] = (0, 100),
            ["HalalPreference"] = (0, 100),
            ["KosherPreference"] = (0, 100),
            ["AllergiesPreference"] = (0, 100),
            ["GlutenFreePreference"] = (50, 80),
            ["DairyFreePreference"] = (50, 80),
            ["NutFreePreference"] = (50, 80),
            ["VegetarianPreference"] = (75, 50)
        };

        private static readonly string[] MeatBasedFood = { "SeafoodPreference" };

        private static readonly string[] VeganBoosted =
        {
            "VegetarianPreference",
            "OrganicPreference",
            "GlutenFreePreference"
        };

        public PreferenceProfile Build(
            UserAccommodationPreferences? accommodation,
            UserFoodPreferences? food,
            UserPersonalization? personalization) =>
            Build(new[] { (accommodation, food, personalization) });

        public PreferenceProfile Build(
            IEnumerable<(UserAccommodationPreferences? Accommodation, UserFoodPreferences? Food, UserPersonalization? Personalization)> preferences)
        {
            var travellers = (preferences ?? Enumerable.Empty<(UserAccommodationPreferences?, UserFoodPreferences?, UserPersonalization?)>())
                .ToList();

            if (travellers.Count == 0)
            {
                return PreferenceProfile.Empty;
            }

            var accommodationItems = Aggregate(
                travellers.Select(traveller => (object?)traveller.Accommodation), DominantAccommodation);

            var foodItems = Aggregate(
                travellers.Select(traveller => (object?)traveller.Food), DominantFood);

            var personalizationItems = Aggregate(
                travellers.Select(traveller => (object?)traveller.Personalization), DominantPersonalization);

            var mandatory = travellers
                .SelectMany(traveller => MandatoryFoodOf(traveller.Food))
                .ToHashSet();

            if (mandatory.Count > 0)
            {
                foodItems = ApplyMandatoryFood(foodItems, mandatory);
            }

            return new PreferenceProfile(accommodationItems, foodItems, personalizationItems);
        }

        private static List<PreferenceItem> Aggregate(IEnumerable<object?> sources, HashSet<string> dominantNames)
        {
            var travellers = sources.ToList();
            var scoresByName = new Dictionary<string, List<int>>();

            foreach (var source in travellers)
            {
                if (source == null)
                {
                    continue;
                }

                foreach (var property in source.GetType().GetProperties().Where(IsPreferenceProperty))
                {
                    if (property.GetValue(source) is not int score || score <= 0)
                    {
                        continue;
                    }

                    if (!scoresByName.TryGetValue(property.Name, out var scores))
                    {
                        scores = new List<int>();
                        scoresByName[property.Name] = scores;
                    }

                    scores.Add(Math.Clamp(score, PreferenceScoring.MinScore, PreferenceScoring.MaxScore));
                }
            }

            var items = new List<PreferenceItem>();

            foreach (var (name, scores) in scoresByName)
            {
                var mean = (double)scores.Sum() / travellers.Count;
                var groupScore = (int)Math.Round((GroupMeanWeight * mean) + (GroupMaxWeight * scores.Max()));

                if (groupScore <= 0)
                {
                    continue;
                }

                var item = new PreferenceItem
                {
                    Name = name,
                    Score = groupScore,
                    IsDominant = dominantNames.Contains(name)
                };

                Reweigh(item);
                items.Add(item);
            }

            return items;
        }

        private static void Reweigh(PreferenceItem item) =>
            item.Weight = item.Score * (item.IsDominant ? DominantMultiplier : 1d);

        private static IEnumerable<string> MandatoryFoodOf(UserFoodPreferences? food)
        {
            if (food == null)
            {
                yield break;
            }

            foreach (var (name, rule) in MandatoryFood)
            {
                if (typeof(UserFoodPreferences).GetProperty(name)?.GetValue(food) is int score &&
                    score > rule.Threshold)
                {
                    yield return name;
                }
            }
        }

        private static List<PreferenceItem> ApplyMandatoryFood(List<PreferenceItem> items, HashSet<string> mandatory)
        {
            var result = new List<PreferenceItem>(items);

            var excludesMeat =
                mandatory.Contains("VeganPreference") ||
                mandatory.Contains("VegetarianPreference");

            if (excludesMeat)
            {
                result.RemoveAll(item => MeatBasedFood.Contains(item.Name));
            }

            if (mandatory.Contains("VeganPreference"))
            {
                foreach (var item in result.Where(item => VeganBoosted.Contains(item.Name)))
                {
                    item.Score = Math.Min(item.Score * VeganBoostMultiplier, PreferenceScoring.MaxScore);
                    Reweigh(item);
                }
            }

            foreach (var name in mandatory)
            {
                var item = result.FirstOrDefault(candidate => candidate.Name == name);

                if (item == null)
                {
                    continue;
                }

                item.IsMandatory = true;
                item.Priority = MandatoryFood[name].Priority;
            }

            return result;
        }

        private static bool IsPreferenceProperty(PropertyInfo property) =>
            property.PropertyType == typeof(int?) &&
            property.Name is not ("UserId" or "Id" or "User");
    }
}
