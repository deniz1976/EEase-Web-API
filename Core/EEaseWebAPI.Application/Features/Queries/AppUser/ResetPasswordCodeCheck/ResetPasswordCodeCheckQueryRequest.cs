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
        public string? code {  get; set; }
        public string? usernameOrEmail { get; set; }
    }
}
