using System;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;

namespace EEaseWebAPI.Application.Exceptions.GetCitiesBySearch
{
    public class GetCitiesBySearchFailedException : BaseException
    {
        public GetCitiesBySearchFailedException() : base("Failed to search cities.", (int)StatusEnum.GetCitiesBySearchFailed)
        {
        }

        public GetCitiesBySearchFailedException(string message) : base(message, (int)StatusEnum.GetCitiesBySearchFailed)
        {
        }

        public GetCitiesBySearchFailedException(string message, Exception innerException) : base(message, (int)StatusEnum.GetCitiesBySearchFailed, innerException)
        {
        }
    }
} 