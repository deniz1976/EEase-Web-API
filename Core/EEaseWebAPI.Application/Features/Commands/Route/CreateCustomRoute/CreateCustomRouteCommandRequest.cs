using EEaseWebAPI.Domain.Entities.Route;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.Route.CreateCustomRoute
{
    public class CreateCustomRouteCommandRequest : IRequest<CreateCustomRouteCommandResponse>
    {
        public List<string> usernames = new List<string>();
        public string? destination { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public PRICE_LEVEL? PRICE_LEVEL { get; set; }

        public string username { get; set; }

    }
}
