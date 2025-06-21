using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions.ResetPassword
{
    public class ResetPasswordCodeNotCorrectException : BaseException
    {
        public ResetPasswordCodeNotCorrectException(string message, int statusenumcode) : base(message, statusenumcode) { }

    }
}
