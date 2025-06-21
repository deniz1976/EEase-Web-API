using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.CheckEmailIsInUse
{
    public class CheckEmailIsInUseQueryRequest : IRequest<CheckEmailIsInUseQueryResponse>
    {
        public string? email {  get; set; }
    }
}
