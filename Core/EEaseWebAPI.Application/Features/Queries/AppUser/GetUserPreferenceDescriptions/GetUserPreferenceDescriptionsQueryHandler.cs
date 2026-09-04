using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetUserPreferenceDescriptions
{
    public class GetUserPreferenceDescriptionsQueryHandler : IRequestHandler<GetUserPreferenceDescriptionsQueryRequest, GetUserPreferenceDescriptionsQueryResponse>
    {
        private readonly IUserService _userService;
        private readonly IHeaderService _headerService;

        public GetUserPreferenceDescriptionsQueryHandler(IUserService userService, IHeaderService headerService)
        {
            _userService = userService;
            _headerService = headerService;
        }

        public async Task<GetUserPreferenceDescriptionsQueryResponse> Handle(GetUserPreferenceDescriptionsQueryRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var descriptions = await _userService.GetUserPreferenceDescriptionsAsync(request.Username);

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