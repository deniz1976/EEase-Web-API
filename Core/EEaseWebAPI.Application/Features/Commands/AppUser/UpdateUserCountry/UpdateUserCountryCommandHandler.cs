using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.UpdateUserCountry;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUserCountry
{
    public class UpdateUserCountryCommandHandler : IRequestHandler<UpdateUserCountryCommandRequest, UpdateUserCountryCommandResponse>
    {
        private readonly IUserService _userService;
        private readonly IHeaderService _headerService;

        public UpdateUserCountryCommandHandler(IUserService userService, IHeaderService headerService)
        {
            _userService = userService;
            _headerService = headerService;
        }

        public async Task<UpdateUserCountryCommandResponse> Handle(UpdateUserCountryCommandRequest request, CancellationToken cancellationToken)
        {
            try
            {
                bool result = await _userService.UpdateUserCountry(request.Username, request.Country);

                return new UpdateUserCountryCommandResponse
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.UpdateUserCountrySuccess),
                    Body = new UpdateUserCountryCommandResponseBody()
                };
            }
            catch (InvalidCountryException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update user country: {ex.Message}", ex);
            }
        }
    }
} 