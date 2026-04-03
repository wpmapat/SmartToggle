using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;



namespace SmartToggle.Models
{
    public class FeatureFlag<T>
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("companyId")]
        public string CompanyId { get; set; }

        [JsonProperty("serviceId")]
        public string ServiceId { get; set; }

        [JsonProperty("flagId")]
        public string FlagId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("defaultValue")]
        public T DefaultValue { get; set; }
    }
}