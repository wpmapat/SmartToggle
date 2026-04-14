using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SmartToggle.Models
{
    public class Service
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [Required]
        [JsonProperty("companyId")]
        public string CompanyId { get; set; } = string.Empty;

        [Required]
        [JsonProperty("servicename")]
        public string ServiceName { get; set; } = string.Empty;

        [Required]
        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("owners")]
        public List<string> Owners { get; set; } = new();
    }
}
