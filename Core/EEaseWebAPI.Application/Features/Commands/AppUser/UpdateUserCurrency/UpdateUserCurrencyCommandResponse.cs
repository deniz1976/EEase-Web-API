using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUserCurrency
{
    public class UpdateUserCurrencyCommandResponse : ApiResponse<UpdateUserCurrencyBody>
    {
    }

    public class UpdateUserCurrencyBody
    {
        public string? Message { get; set; }
    }
}
