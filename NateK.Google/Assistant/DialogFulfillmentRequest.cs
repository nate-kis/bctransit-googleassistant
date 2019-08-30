using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace NateK.Google.Assistant
{

    public class DialogFulfillmentRequest<T> where T : DialogFulfillmentResultParameters
    {
        [JsonProperty("responseId")]
        public string ResponseId { get; set; }
        [JsonProperty("session")]
        public string Session { get; set; }
        [JsonProperty("queryResult")]
        public DialogFulfillmentResult<T> QueryResult { get; set; }
    }

    public class DialogFulfillmentResultParameters { }

    public class DialogFulfillmentResult<T> where T : DialogFulfillmentResultParameters
    {
        [JsonProperty("queryText")]
        public string QueryText { get; set; }
        [JsonProperty("allRequiredParamsPresent")]
        public bool AllRequiredParamsPresent { get; set; }
        [JsonProperty("intent")]
        public DialogFulfillmentIntent Intent { get; set; }
        [JsonProperty("outputContexts")]
        public List<DialogFulfullmentOutputContext> OutputContexts { get; set; }
        public T Parameters { get; set; }
    }

    public class DialogFulfillmentIntent
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
    }

    public class DialogFulfillmentResponse
    {
        [JsonProperty("fulfillmentText")]
        public string FulfillmentText { get; set; }
        [JsonProperty("source")]
        public string Source { get; set; } = "webhook";
        [JsonProperty("outputContexts")]
        public List<DialogFulfullmentOutputContext> OutputContexts { get; set; }
    }

    public class DialogFulfullmentOutputContext
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("lifespanCount")]
        public int LifespanCount { get; set; }
        [JsonProperty("parameters")]
        public dynamic Parameters { get; set; }
    }

}
