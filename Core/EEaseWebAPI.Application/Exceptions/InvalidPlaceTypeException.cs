using System;

namespace EEaseWebAPI.Application.Exceptions
{
    public class InvalidPlaceTypeException : Exception
    {
        public int ErrorCode { get; }

        public InvalidPlaceTypeException(string message) : base(message)
        {
            ErrorCode = 98;
        }

        public InvalidPlaceTypeException(string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = 98;
        }
    }
} 