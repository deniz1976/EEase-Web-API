using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.VerifyPasswordResetCode
{
    public class VerifyPasswordResetCodeQueryRequest : IRequest<VerifyPasswordResetCodeQueryResponse>
    {
        public string Code { get; set; } = string.Empty;
        public string UsernameOrEmail { get; set; } = string.Empty;
    }
}
