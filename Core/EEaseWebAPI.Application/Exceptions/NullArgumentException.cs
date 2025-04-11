using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions
{
    public class NullArgumentException : BaseException
    {
        public NullArgumentException(string message, int statusEnum) : base(message, statusEnum)
        {
        }
    }
}
