using EEaseWebAPI.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions
{
    public class GeminiAPIKeyLimitExceededException : BaseException
    {
        public GeminiAPIKeyLimitExceededException() : base("All available Gemini API keys have reached their usage limit.", (int)StatusEnum.GeminiAPIKeyLimitExceeded)
        {
        }

        public GeminiAPIKeyLimitExceededException(string message) : base(message, (int)StatusEnum.GeminiAPIKeyLimitExceeded)
        {
        }

        public GeminiAPIKeyLimitExceededException(string message, Exception innerException) : base(message, (int)StatusEnum.GeminiAPIKeyLimitExceeded, innerException)
        {
        }
    }
} 