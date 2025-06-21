using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions
{
    public class BaseException : Exception
    {
        public int? EnumStatusCode {get; set;}

        public BaseException(string message, int statusEnum) : base(message)
        {
            EnumStatusCode = statusEnum;
        }

        public BaseException(int statusEnum) 
        {
            EnumStatusCode = statusEnum;
        }

        public BaseException(string message , int statusEnum , Exception innerException) : base(message,innerException) 
        {
            EnumStatusCode = statusEnum;
        }
    }
}
