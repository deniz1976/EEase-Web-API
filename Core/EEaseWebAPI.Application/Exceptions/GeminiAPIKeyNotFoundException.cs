using EEaseWebAPI.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions
{
    public class GeminiAPIKeyNotFoundException : BaseException
    {
        public GeminiAPIKeyNotFoundException() : base("No available Gemini API key found.", (int)StatusEnum.GeminiAPIKeyNotFound)
        {
        }

        public GeminiAPIKeyNotFoundException(string message) : base(message, (int)StatusEnum.GeminiAPIKeyNotFound)
        {
        }

        public GeminiAPIKeyNotFoundException(string message, Exception innerException) : base(message, (int)StatusEnum.GeminiAPIKeyNotFound, innerException)
        {
        }
    }
} 