using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.GetAllCountries;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.AllWorldCities.GetAllCountries
{
    public class GetAllCountriesQueryHandler : IRequestHandler<GetAllCountriesQueryRequest, GetAllCountriesQueryResponse>
    {
        private readonly ICityService _cityService;
        private readonly IHeaderService _headerService;

        public GetAllCountriesQueryHandler(ICityService cityService, IHeaderService headerService)
        {
            _cityService = cityService;
            _headerService = headerService;
        }

        public async Task<GetAllCountriesQueryResponse> Handle(GetAllCountriesQueryRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var countries = await _cityService.GetAllCountries();

                return new GetAllCountriesQueryResponse
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.GetAllCountriesSuccess),
                    Body = new GetAllCountriesQueryResponseBody { Countries = countries }
                };
            }
            catch (Exception ex)
            {
                throw new GetAllCountriesFailedException($"Failed to get all countries: {ex.Message}", ex);
            }
        }
    }
} 