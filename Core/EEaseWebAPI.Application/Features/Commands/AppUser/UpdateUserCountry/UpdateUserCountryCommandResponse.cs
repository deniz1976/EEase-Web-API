using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUserCountry
{
    public class UpdateUserCountryCommandResponse : ApiResponse<UpdateUserCountryCommandResponseBody>
    {
    }

    public class UpdateUserCountryCommandResponseBody
    {
        public string? Message { get; set; }
    }
}
