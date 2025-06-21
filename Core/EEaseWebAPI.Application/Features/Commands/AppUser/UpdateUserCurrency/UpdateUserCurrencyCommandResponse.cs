using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUserCurrency
{
    public class UpdateUserCurrencyCommandResponse
    {
        public UpdateUserCurrencyResponse? Response { get; set; }
    }

    public class UpdateUserCurrencyResponse
    {
        public Header? Header { get; set; }
        public UpdateUserCurrencyBody? Body { get; set; }
    }

    public class UpdateUserCurrencyBody
    {
        public string? Message { get; set; }
    }
}
