namespace EEaseWebAPI.Domain.Entities.Route
{
    public interface IHasWeather
    {
        Weather? Weather { get; set; }
    }
}
