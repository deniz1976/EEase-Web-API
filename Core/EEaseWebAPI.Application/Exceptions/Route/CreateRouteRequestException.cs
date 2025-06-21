using EEaseWebAPI.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions.Route
{
    public class CreateRouteRequestException : BaseException
    {
        public CreateRouteRequestException() : base("Request is not valid.", (int)StatusEnum.RouteRequestIsNotValid)
        {
        }

        public CreateRouteRequestException(string message) : base(message, (int)StatusEnum.RouteRequestIsNotValid)
        {
        }

        public CreateRouteRequestException(string message, Exception innerException) : base(message, (int)StatusEnum.RouteRequestIsNotValid, innerException)
        {
        }
    }
}
