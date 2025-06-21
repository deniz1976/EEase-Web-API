using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUserCurrency
{
    public class UpdateUserCurrencyCommandRequest : IRequest<UpdateUserCurrencyCommandResponse>
    {
        public string? Username { get; set; }
        public string? CurrencyCode { get; set; }
    }
}
