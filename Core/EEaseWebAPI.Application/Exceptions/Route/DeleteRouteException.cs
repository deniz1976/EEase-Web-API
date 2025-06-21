using EEaseWebAPI.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions.Route
{
    public class DeleteRouteException : BaseException
    {
        public DeleteRouteException() : base("Unauthorized to delete route", (int)StatusEnum.UnauthorizedToDeleteRoute)
        {
        }

        public DeleteRouteException(string message) : base(message, (int)StatusEnum.UnauthorizedToDeleteRoute)
        {
        }

        public DeleteRouteException(string message, Exception innerException) : base(message, (int)StatusEnum.UnauthorizedToDeleteRoute, innerException)
        {
        }
    }
}
