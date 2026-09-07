using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Exceptions.Route
{
    public class RouteGenerationException : BaseException
    {
        public RouteGenerationException(string message)
            : base(message, (int)StatusEnum.RouteGenerationFailed)
        {
        }

        public RouteGenerationException(string message, Exception innerException)
            : base(message, (int)StatusEnum.RouteGenerationFailed, innerException)
        {
        }
    }
}
