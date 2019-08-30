using NateK.Google.Assistant;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace NextBusFncApp.Models
{
    public class GenericActionParameters : DialogFulfillmentResultParameters
    {
        [JsonProperty("action")]
        public string Action { get; set; }
    }
}
