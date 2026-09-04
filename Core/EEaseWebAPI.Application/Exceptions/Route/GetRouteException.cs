using EEaseWebAPI.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions.Route
{
    public class GetRouteException : BaseException
    {

        public GetRouteException() : base("Unauthorized to view route", (int)StatusEnum.UnauthorizedToViewRoute)
        {
        }

        public GetRouteException(string message) : base(message, (int)StatusEnum.UnauthorizedToViewRoute)
        {
        }

        public GetRouteException(string message, Exception innerException) : base(message, (int)StatusEnum.UnauthorizedToViewRoute, innerException)
        {
        }
    }
}
