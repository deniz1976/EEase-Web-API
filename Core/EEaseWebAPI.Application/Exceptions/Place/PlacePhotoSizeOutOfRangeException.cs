using EEaseWebAPI.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions.Place
{
    public class PlacePhotoSizeOutOfRangeException : BaseException
    {
        public PlacePhotoSizeOutOfRangeException() : base("Height and width should be >= 0 and < 4800.", (int)StatusEnum.RouteComponentPhotoRequestOutOfRange)
        {
        }

        public PlacePhotoSizeOutOfRangeException(string message) : base(message, (int)StatusEnum.RouteComponentPhotoRequestOutOfRange)
        {
        }

        public PlacePhotoSizeOutOfRangeException(string message, Exception innerException) : base(message, (int)StatusEnum.RouteComponentPhotoRequestOutOfRange, innerException)
        {
        }
    }
}
