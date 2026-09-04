using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetUserCurrency
{
    public class GetUserCurrencyQueryHandler : IRequestHandler<GetUserCurrencyQueryRequest, GetUserCurrencyQueryResponse>
    {
        private readonly IUserService _userService;
        private readonly IHeaderService _headerService;

        public GetUserCurrencyQueryHandler(IUserService userService, IHeaderService headerService)
        {
            _userService = userService;
            _headerService = headerService;
        }

        public async Task<GetUserCurrencyQueryResponse> Handle(GetUserCurrencyQueryRequest request, CancellationToken cancellationToken)
        {
            string currency = await _userService.GetUserCurrencyAsync(request.Username);

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