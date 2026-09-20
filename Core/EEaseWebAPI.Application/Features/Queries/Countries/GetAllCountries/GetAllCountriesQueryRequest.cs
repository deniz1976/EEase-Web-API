using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.Countries.GetAllCountries
{
    public class GetAllCountriesQueryRequest : IRequest<GetAllCountriesQueryResponse>
    {
    }
}
