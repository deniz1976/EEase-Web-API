using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions.Route
{
    public class RouteNotFoundException : BaseException
    {
        public RouteNotFoundException(int statusEnum) : base(statusEnum)
        {
        }

        public RouteNotFoundException(string message, int statusEnum) : base(message, statusEnum)
        {
        }

        public RouteNotFoundException(string message, int statusEnum, Exception innerException) : base(message, statusEnum, innerException)
        {
        }
    }
}
