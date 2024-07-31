using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PayTech.BackOffice.Core.DomainServices
{
    public class GeolocationService 
    {
        private readonly HttpClient _httpClient;

        public GeolocationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<GeoLocationData> GetGeolocationDataAsync(string ipAddress)
        {
            var response = await _httpClient.GetAsync($"https://ipapi.co/{ipAddress}/json/");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GeoLocationData>(content);
        }
    }

    public class GeoLocationData
    {
        public string Ip { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
