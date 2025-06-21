using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions.DeleteUser
{
    public class DeleteUserCodeNotCorrectException : BaseException
    {
        public DeleteUserCodeNotCorrectException(string message, int statusenumcode) : base(message, statusenumcode) { }

    }
}
