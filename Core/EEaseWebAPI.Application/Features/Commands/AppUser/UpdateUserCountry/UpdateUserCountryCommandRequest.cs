using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUserCountry
{
    public class UpdateUserCountryCommandRequest : IRequest<UpdateUserCountryCommandResponse>
    {
        public string Country { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }
}
