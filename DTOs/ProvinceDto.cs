using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Residence.DTOs;

public class District
    {
        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonProperty("code")]
        [JsonPropertyName("code")]
        public int? Code { get; set; }

        [JsonProperty("division_type")]
        [JsonPropertyName("division_type")]
        public string? DivisionType { get; set; }

        [JsonProperty("codename")]
        [JsonPropertyName("codename")]
        public string? Codename { get; set; }

        [JsonProperty("province_code")]
        [JsonPropertyName("province_code")]
        public int? ProvinceCode { get; set; }

        [JsonProperty("wards")]
        [JsonPropertyName("wards")]
        public List<Ward>? Wards { get; set; }
    }

    public class Province
    {
        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonProperty("code")]
        [JsonPropertyName("code")]
        public int? Code { get; set; }

        [JsonProperty("division_type")]
        [JsonPropertyName("division_type")]
        public string? DivisionType { get; set; }

        [JsonProperty("codename")]
        [JsonPropertyName("codename")]
        public string? Codename { get; set; }

        [JsonProperty("phone_code")]
        [JsonPropertyName("phone_code")]
        public int? PhoneCode { get; set; }

        [JsonProperty("districts")]
        [JsonPropertyName("districts")]
        public List<District>? Districts { get; set; }
    }

    public class Ward
    {
        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonProperty("code")]
        [JsonPropertyName("code")]
        public int? Code { get; set; }

        [JsonProperty("division_type")]
        [JsonPropertyName("division_type")]
        public string? DivisionType { get; set; }

        [JsonProperty("codename")]
        [JsonPropertyName("codename")]
        public string? Codename { get; set; }

        [JsonProperty("district_code")]
        [JsonPropertyName("district_code")]
        public int? DistrictCode { get; set; }
    }