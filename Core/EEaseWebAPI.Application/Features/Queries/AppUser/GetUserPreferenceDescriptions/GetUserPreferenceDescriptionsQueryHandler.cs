using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetUserPreferenceDescriptions
{
    public class GetUserPreferenceDescriptionsQueryHandler : IRequestHandler<GetUserPreferenceDescriptionsQueryRequest, GetUserPreferenceDescriptionsQueryResponse>
    {
        private readonly IUserPreferenceService _preferenceService;
        private readonly IHeaderService _headerService;

        public GetUserPreferenceDescriptionsQueryHandler(IUserPreferenceService preferenceService, IHeaderService headerService)
        {
            _preferenceService = preferenceService;
            _headerService = headerService;
        }

        public async Task<GetUserPreferenceDescriptionsQueryResponse> Handle(GetUserPreferenceDescriptionsQueryRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var descriptions = await _preferenceService.GetDescriptionsAsync(request.Username);

                return new GetUserPreferenceDescriptionsQueryResponse
                {
                    response = new GetUserPreferenceDescriptionsResponse
                    {
                        Header = _headerService.HeaderCreate((int)StatusEnum.PreferenceDescriptionsRetrievedSuccessfully),
                        Body = descriptions
                    }
                };
            }
            catch (Exception ex)
            {
                throw new BaseException(ex.Message, (int)StatusEnum.PreferenceDescriptionsRetrievalFailed);
            }
        }
    }
}
