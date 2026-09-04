using EEaseWebAPI.Application.Repositories;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Persistence.Repositories
{
    public class StandardRouteReadRepository : ReadRepository<StandardRoute>, IStandardRouteReadRepository
    {
        public StandardRouteReadRepository(EEaseAPIDbContext context) : base(context)
        {
        }
    }
}
