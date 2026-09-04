using EEaseWebAPI.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions
{
    public class TargetIsUserException : BaseException
    {
        public TargetIsUserException() : base("Target and user are the same.", (int)StatusEnum.TargetIsUser)
        {
        }

        public TargetIsUserException(string message) : base(message, (int)StatusEnum.TargetIsUser)
        {
        }

        public TargetIsUserException(string message, Exception innerException) : base(message, (int)StatusEnum.TargetIsUser, innerException)
        {
        }
    }
}
