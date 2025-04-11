using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions.DeleteUser
{
    public class UserStatusAlreadyFalseException: BaseException
    {
        public UserStatusAlreadyFalseException(string message, int statusenumcode) : base(message, statusenumcode) { }

    }
}
