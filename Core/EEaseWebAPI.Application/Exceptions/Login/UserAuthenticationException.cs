using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions.Login
{
    public class UserAuthenticationException : BaseException
    {
        public UserAuthenticationException(string message, int statusenumcode) : base(message, statusenumcode) { }


    }
}
