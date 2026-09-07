using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetUserCurrency
{
    public class GetUserCurrencyQueryHandler : IRequestHandler<GetUserCurrencyQueryRequest, GetUserCurrencyQueryResponse>
    {
        private readonly IUserProfileService _profileService;
        private readonly IHeaderService _headerService;

        public GetUserCurrencyQueryHandler(IUserProfileService profileService, IHeaderService headerService)
        {
            _profileService = profileService;
            _headerService = headerService;
        }

        public async Task<GetUserCurrencyQueryResponse> Handle(GetUserCurrencyQueryRequest request, CancellationToken cancellationToken)
        {
            string currency = await _profileService.GetUserCurrencyAsync(request.Username);

            return new GetUserCurrencyQueryResponse
            {

                    Header = _headerService.HeaderCreate((int)StatusEnum.UserCurrencyReceivedSuccessfully),
                    Body = new GetUserCurrencyBody
                    {
                        Currency = currency
                    }

            };
        }
    }
}
