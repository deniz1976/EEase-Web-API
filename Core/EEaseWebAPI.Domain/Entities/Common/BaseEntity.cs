using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace EEaseWebAPI.Domain.Entities.Common
{
    public class BaseEntity
    {
        [JsonIgnore]
        public Guid Id { get; set; }

        [JsonIgnore]
        public DateTime CreatedDate { get; set; }

        [JsonIgnore]
        public DateTime UpdatedDate { get; set; }
    }
}
