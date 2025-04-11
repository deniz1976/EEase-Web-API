using EEaseWebAPI.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EEaseWebAPI.Domain.Entities.Route
{
    public class PaymentOptions : BaseEntity
    {
        [JsonPropertyName("acceptsCreditCards")]
        public string? AcceptsCreditCards { get; set; }

        [JsonPropertyName("acceptsDebitCards")]
        public string? AcceptsDebitCards { get; set; }

        [JsonPropertyName("acceptsCashOnly")]
        public string? AcceptsCashOnly { get; set; }
    }
}
