using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions.ChangePassword
{
    public class PasswordChangeException : BaseException
    {
        public PasswordChangeException(string message, int statusenumcode) : base(message,statusenumcode) { }


    }
}
