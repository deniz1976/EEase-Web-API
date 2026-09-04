using EEaseWebAPI.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions.GetRouteComponent
{
    public class RouteComponentRequestOutOfRangeException : BaseException
    {
        public RouteComponentRequestOutOfRangeException() : base("Height and width should be >= 0 and < 4800.", (int)StatusEnum.RouteComponentPhotoRequestOutOfRange)
        {
        }

        public RouteComponentRequestOutOfRangeException(string message) : base(message, (int)StatusEnum.RouteComponentPhotoRequestOutOfRange)
        {
        }

        public RouteComponentRequestOutOfRangeException(string message, Exception innerException) : base(message, (int)StatusEnum.RouteComponentPhotoRequestOutOfRange, innerException)
        {
        }
    }
}
