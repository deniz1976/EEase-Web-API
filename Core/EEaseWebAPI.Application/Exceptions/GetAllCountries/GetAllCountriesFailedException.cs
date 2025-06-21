using System;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;

namespace EEaseWebAPI.Application.Exceptions.GetAllCountries
{
    public class GetAllCountriesFailedException : BaseException
    {
        public GetAllCountriesFailedException() : base("Failed to get all countries.", (int)StatusEnum.GetAllCountriesFailed)
        {
        }

        public GetAllCountriesFailedException(string message) : base(message, (int)StatusEnum.GetAllCountriesFailed)
        {
        }

        public GetAllCountriesFailedException(string message, Exception innerException) : base(message, (int)StatusEnum.GetAllCountriesFailed, innerException)
        {
        }
    }
} 