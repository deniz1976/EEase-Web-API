namespace EEaseWebAPI.Application.Exceptions.Route
{
    public class PlaceNotFoundInRouteException : BaseException
    {
        public PlaceNotFoundInRouteException(string message)
            : base(message, (int)Enums.StatusEnum.RouteNotFound) { }
    }
}
