using EEaseWebAPI.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions.Route
{
    public class InvalidRouteStatusException : BaseException
    {

        public InvalidRouteStatusException() : base("Invalid route status code", (int)StatusEnum.InvalidStatusRoute)
        {
        }

        public InvalidRouteStatusException(string message) : base(message, (int)StatusEnum.InvalidStatusRoute)
        {
        }

        public InvalidRouteStatusException(string message, Exception innerException) : base(message, (int)StatusEnum.InvalidStatusRoute, innerException)
        {
        }
    }
}
