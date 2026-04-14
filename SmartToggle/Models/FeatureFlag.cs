using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SmartToggle.Models
{
    public class FeatureFlag<T>
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [Required]
        [JsonProperty("companyId")]
        public string CompanyId { get; set; } = string.Empty;

        [Required]
        [JsonProperty("serviceId")]
        public string ServiceId { get; set; } = string.Empty;

        [Required]
        [JsonProperty("flagId")]
        public string FlagId { get; set; } = string.Empty;

        [Required]
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [Required]
        [JsonProperty("defaultValue")]
        public T DefaultValue { get; set; } = default!;
    }
}
