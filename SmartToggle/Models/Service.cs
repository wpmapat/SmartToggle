using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
namespace SmartToggle.Models
{
    public class Service
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("companyId")]
        public string CompanyId { get; set; }

        [JsonProperty("servicename")]
        public string ServiceName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("owners")]
        public List<string> Owners { get; set; }
    }
}