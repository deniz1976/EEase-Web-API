using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions.UpdateUser
{
    public class UpdateUserSaveException : BaseException
    {
        public UpdateUserSaveException(string message, int statusenumcode) : base(message, statusenumcode) { }

    }
}
