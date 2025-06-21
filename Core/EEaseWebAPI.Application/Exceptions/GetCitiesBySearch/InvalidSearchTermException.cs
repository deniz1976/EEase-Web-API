using System;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;

namespace EEaseWebAPI.Application.Exceptions.GetCitiesBySearch
{
    public class InvalidSearchTermException : BaseException
    {
        public InvalidSearchTermException() : base("Search term must be at least 2 characters long.", (int)StatusEnum.InvalidSearchTerm)
        {
        }

        public InvalidSearchTermException(string message) : base(message, (int)StatusEnum.InvalidSearchTerm)
        {
        }

        public InvalidSearchTermException(string message, Exception innerException) : base(message, (int)StatusEnum.InvalidSearchTerm, innerException)
        {
        }
    }
} 