using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.StatusCheck
{
    public class StatusCheckQueryRequest : IRequest<StatusCheckQueryResponse>
    {
        public string? username {  get; set; }
    }
}
