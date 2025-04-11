using EEaseWebAPI.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Domain.Entities.Route
{
    public class Weather : BaseEntity
    {
        public int? Degree {  get; set; }
        public string? Description { get; set; }
        public string? Warning { get; set; }
        public DateOnly? Date { get; set; }
    }
}
