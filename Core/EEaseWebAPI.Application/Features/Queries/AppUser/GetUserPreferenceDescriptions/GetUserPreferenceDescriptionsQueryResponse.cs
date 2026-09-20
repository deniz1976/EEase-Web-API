using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetUserPreferenceDescriptions
{
    public class GetUserPreferenceDescriptionsQueryResponse : ApiResponse<GetUserPreferenceDescriptionsBody>
    {
    }

    public class GetUserPreferenceDescriptionsBody
    {
        public List<PreferenceDetail> PersonalizationPreferences { get; set; } = new();
        public List<PreferenceDetail> FoodPreferences { get; set; } = new();
        public List<PreferenceDetail> AccommodationPreferences { get; set; } = new();
    }

    public class PreferenceDetail
    {
        public string? Description { get; set; }
        public int? Value { get; set; }
    }
}
