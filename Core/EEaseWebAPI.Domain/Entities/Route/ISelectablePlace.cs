namespace EEaseWebAPI.Domain.Entities.Route
{
    public interface ISelectablePlace
    {
        Guid Id { get; set; }

        string? GoogleId { get; set; }

        PRICE_LEVEL? _PRICE_LEVEL { get; set; }
    }
}
