using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.StatusCheck
{
    public class StatusCheckQueryResponse
    {
        public Application.MapEntities.StatusCheck.StatusCheck StatusCheck { get; set; }
    }
}
