using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions
{
    public class EmailConfirmException : BaseException
    {
        public EmailConfirmException(string message, int statusenumcode) : base(message, statusenumcode) { }

    }
}
