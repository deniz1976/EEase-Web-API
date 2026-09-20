using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.ResetPasswordCodeCheck
{
    public class ResetPasswordCodeCheckQueryRequest : IRequest<ResetPasswordCodeCheckQueryResponse>
    {
        public string Code { get; set; } = string.Empty;
        public string UsernameOrEmail { get; set; } = string.Empty;
    }
}
