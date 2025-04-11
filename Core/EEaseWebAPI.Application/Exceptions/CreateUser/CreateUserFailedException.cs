using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions.CreateUser
{
    public class CreateUserFailedException : BaseException
    {
        public CreateUserFailedException(string message, int statusenumcode) : base(message, statusenumcode) { }

    }
}
