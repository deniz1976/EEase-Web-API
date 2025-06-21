using EEaseWebAPI.Application.Common.Models.Pagination;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.Currency.GetCurrencies
{
    public class GetCurrenciesQueryRequest : PaginationRequest, IRequest<GetCurrenciesQueryResponse>
    {
        public string? username { get; set; }
    }
}
