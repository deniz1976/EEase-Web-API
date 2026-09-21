using System;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;

namespace EEaseWebAPI.Application.Exceptions.Cities
{
    public class SearchCitiesFailedException : BaseException
    {
        public SearchCitiesFailedException() : base("Failed to search cities.", (int)StatusEnum.GetCitiesBySearchFailed)
        {
        }

        public SearchCitiesFailedException(string message) : base(message, (int)StatusEnum.GetCitiesBySearchFailed)
        {
        }

        public SearchCitiesFailedException(string message, Exception innerException) : base(message, (int)StatusEnum.GetCitiesBySearchFailed, innerException)
        {
        }
    }
}
