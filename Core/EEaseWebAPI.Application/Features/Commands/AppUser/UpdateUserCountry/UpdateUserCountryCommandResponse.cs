using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUserCountry
{
    public class UpdateUserCountryCommandResponse
    {
        public Header? Header { get; set; }
        public UpdateUserCountryCommandResponseBody? Body { get; set; }
    }

    public class UpdateUserCountryCommandResponseBody
    {
        public string Message { get; set; } = "User country preference updated successfully.";
    }
} 