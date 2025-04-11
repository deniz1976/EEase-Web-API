using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions.Login
{
    public class UserNotFoundException : BaseException
    {
        public UserNotFoundException(string message, int statusenumcode) : base(message, statusenumcode) { }

    }
}
