using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions.ChangePassword
{
    public class InvalidPasswordException: BaseException
    {
        public InvalidPasswordException(string message , int statusenumcode) : base(message, statusenumcode) { }
        

        

        
    }
}
