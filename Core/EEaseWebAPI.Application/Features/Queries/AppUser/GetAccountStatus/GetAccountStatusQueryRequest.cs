using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetAccountStatus
{
    public class GetAccountStatusQueryRequest : IRequest<GetAccountStatusQueryResponse>
    {
        public string Username { get; set; } = string.Empty;
    }
}
