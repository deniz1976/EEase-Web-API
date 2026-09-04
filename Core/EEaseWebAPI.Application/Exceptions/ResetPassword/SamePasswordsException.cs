using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions.ResetPassword
{
    public class SamePasswordsException: BaseException
    {
        public SamePasswordsException(string message, int statusenumcode) : base(message, statusenumcode) { }

    }
}
