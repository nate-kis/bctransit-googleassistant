using NateK.Google.Assistant;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace NextBusFncApp.Models
{
    public class NextBusParameters : DialogFulfillmentResultParameters
    {
        [JsonProperty("location")]
        public string Location { get; set; }
    }

    public class NextBusLoadParameters : NextBusParameters
    {
        [JsonProperty("patternId")]
        public int PatternId { get; set; }
    }
}
