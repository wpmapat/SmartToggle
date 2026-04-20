using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SmartToggle.Models
{
    public class Company
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [Required]
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("ownerId")]
        public string OwnerId { get; set; } = string.Empty;

        [JsonProperty("services")]
        public List<Service> Services { get; set; } = new();
    }
}
