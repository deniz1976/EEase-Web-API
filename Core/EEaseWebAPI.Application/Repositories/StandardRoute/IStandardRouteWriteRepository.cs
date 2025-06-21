using EEaseWebAPI.Domain.Entities.Route;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Repositories
{
    //hepsinin namespace repositories altında tut
    public interface IStandardRouteWriteRepository: IWriteRepository<StandardRoute>
    {
    }
}
