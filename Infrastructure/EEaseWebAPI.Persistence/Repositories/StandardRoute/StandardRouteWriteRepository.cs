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
    //namespace repo altında tut 
    public class StandardRouteWriteRepository : WriteRepository<StandardRoute>, IStandardRouteWriteRepository
    {
        public StandardRouteWriteRepository(EEaseAPIDbContext context) : base(context)
        {
        }
    }
}
