using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUserCountry
{
    public class UpdateUserCountryCommandRequest : IRequest<UpdateUserCountryCommandResponse>
    {
        public string Country { get; set; }
        public string Username { get; set; }
    }
} 