using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.GetCitiesBySearch;
using EEaseWebAPI.Application.MapEntities.Cities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.Cities.GetCitiesBySearch
{
    public class GetCitiesBySearchQueryHandler : IRequestHandler<GetCitiesBySearchQueryRequest, GetCitiesBySearchQueryResponse>
    {
        private readonly ICityService _cityService;
        private readonly IHeaderService _headerService;

        public GetCitiesBySearchQueryHandler(ICityService cityService, IHeaderService headerService)
        {
            _cityService = cityService;
            _headerService = headerService;
        }

        public async Task<GetCitiesBySearchQueryResponse> Handle(GetCitiesBySearchQueryRequest request, CancellationToken cancellationToken)
        {
            if (request == null || string.IsNullOrEmpty(request.SearchTerm) || request.SearchTerm.Length < 2)
                throw new InvalidSearchTermException();

            (List<CityDto> cities, int totalCount) = await _cityService.GetCitiesBySearchAsync(
                request.SearchTerm, 
                request.PageSize, 
                request.PageNumber,
                request.Username);
            
            int totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return new GetCitiesBySearchQueryResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.GetCitiesBySearchSuccess),
                Body = new GetCitiesBySearchQueryResponseBody
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