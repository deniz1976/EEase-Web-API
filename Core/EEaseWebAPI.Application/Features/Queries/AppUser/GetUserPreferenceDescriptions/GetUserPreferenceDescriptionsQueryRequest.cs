using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetUserPreferenceDescriptions
{
    public class GetUserPreferenceDescriptionsQueryRequest : IRequest<GetUserPreferenceDescriptionsQueryResponse>
    {
        public string? Username { get; set; }
    }
} 