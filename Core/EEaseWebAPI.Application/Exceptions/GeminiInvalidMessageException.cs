using EEaseWebAPI.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions
{
    public class GeminiInvalidMessageException : BaseException
    {
        public GeminiInvalidMessageException() : base("The provided message is invalid or empty.", (int)StatusEnum.GeminiInvalidMessage)
        {
        }

        public GeminiInvalidMessageException(string message) : base(message, (int)StatusEnum.GeminiInvalidMessage)
        {
        }

        public GeminiInvalidMessageException(string message, Exception innerException) : base(message, (int)StatusEnum.GeminiInvalidMessage, innerException)
        {
        }
    }
} 