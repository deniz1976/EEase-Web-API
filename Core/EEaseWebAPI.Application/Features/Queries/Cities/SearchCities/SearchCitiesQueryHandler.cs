using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.GetCitiesBySearch;
using EEaseWebAPI.Application.MapEntities.Cities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.Cities.SearchCities
{
    public class SearchCitiesQueryHandler : IRequestHandler<SearchCitiesQueryRequest, SearchCitiesQueryResponse>
    {
        private readonly ICityService _cityService;
        private readonly IHeaderService _headerService;

        public SearchCitiesQueryHandler(ICityService cityService, IHeaderService headerService)
        {
            _cityService = cityService;
            _headerService = headerService;
        }

        public async Task<SearchCitiesQueryResponse> Handle(SearchCitiesQueryRequest request, CancellationToken cancellationToken)
        {
            (List<CityDto> cities, int totalCount) = await _cityService.SearchCitiesAsync(
                request.SearchTerm,
                request.PageSize,
                request.PageNumber,
                request.Username,
                cancellationToken);

            int totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return new SearchCitiesQueryResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.GetCitiesBySearchSuccess),
                Body = new SearchCitiesQueryResponseBody
                {
                    Cities = cities,
                    TotalCount = totalCount,
                    PageSize = request.PageSize,
                    CurrentPage = request.PageNumber,
                    TotalPages = totalPages
                }
            };
        }
    }
}
