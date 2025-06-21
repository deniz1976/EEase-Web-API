using EEaseWebAPI.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Exceptions
{
    public class GeminiAIServiceException : BaseException
    {
        public GeminiAIServiceException() : base("An error occurred in the Gemini AI service.", (int)StatusEnum.GeminiAIServiceError)
        {
        }

        public GeminiAIServiceException(string message) : base(message, (int)StatusEnum.GeminiAIServiceError)
        {
        }

        public GeminiAIServiceException(string message, Exception innerException) : base(message, (int)StatusEnum.GeminiAIServiceError, innerException)
        {
        }
    }
} 