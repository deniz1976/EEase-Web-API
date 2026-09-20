using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.SetUserPhoto
{
    public class SetUserPhotoCommandResponse : ApiResponse<SetUserPhotoCommandResponseBody>
    {
    }

    public class SetUserPhotoCommandResponseBody
    {
        public string? PhotoPath { get; set; }
    }
}
