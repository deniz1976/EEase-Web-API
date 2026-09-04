using EEaseWebAPI.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions
{
    public class GeminiAPIResponseParseException : BaseException
    {
        public GeminiAPIResponseParseException() : base("Failed to parse Gemini AI API response.", (int)StatusEnum.GeminiAPIResponseParseError)
        {
        }

        public GeminiAPIResponseParseException(string message) : base(message, (int)StatusEnum.GeminiAPIResponseParseError)
        {
        }

        public GeminiAPIResponseParseException(string message, Exception innerException) : base(message, (int)StatusEnum.GeminiAPIResponseParseError, innerException)
        {
        }
    }
} 