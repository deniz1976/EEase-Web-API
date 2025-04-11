using System;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;

namespace EEaseWebAPI.Application.Exceptions.UpdateUserCountry
{
    public class InvalidCountryException : BaseException
    {
        public InvalidCountryException() : base("Selected country is not in the available countries list.", (int)StatusEnum.InvalidCountry)
        {
        }

        public InvalidCountryException(string message) : base(message, (int)StatusEnum.InvalidCountry)
        {
        }

        public InvalidCountryException(string message, Exception innerException) : base(message, (int)StatusEnum.InvalidCountry, innerException)
        {
        }
    }
} 