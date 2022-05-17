using Talabatak.Models.DTOs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Talabatak.Helpers
{
    public static class Distance
    {
        public static double GetDistanceBetweenTwoLocations(double Latitude1, double Longitude1, double Latitude2, double Longitude2)
        {
            double R = 6371;
            var lat = (Latitude2 - Latitude1).ToRadians();
            var lng = (Longitude2 - Longitude1).ToRadians();
            var h1 = Math.Sin(lat / 2) * Math.Sin(lat / 2) +
                          Math.Cos(Latitude1.ToRadians()) * Math.Cos(Latitude2.ToRadians()) *
                          Math.Sin(lng / 2) * Math.Sin(lng / 2);
            var h2 = 2 * Math.Asin(Math.Min(1, Math.Sqrt(h1)));
            return R * h2;
        }
        public static async Task<EstimatedValueDTO> GetEstimatedDataBetweenTwoLocations(double Latitude1, double Longitude1, double Latitude2, double Longitude2)
        {
            string GoogleDistanceKey = ConfigurationManager.AppSettings["GOOGLE_DISTANCE_MATRIX_KEY"];
            string apiUrl = $"https://maps.googleapis.com/maps/api/distancematrix/json?origins={Latitude1},{Longitude1}&destinations={Latitude2},{Longitude2}&key={GoogleDistanceKey}";
            int TotalEstimatedDistance = 0;
            int TotalEstimatedTime = 0;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<DistanceMatrixDTO>(data);
                        if (result != null && result.rows != null && result.rows.Count > 0 && !result.rows.Any(x => x == null) && !result.rows.FirstOrDefault().elements.Any(x => x.distance == null) && !result.rows.FirstOrDefault().elements.Any(x => x.duration == null))
                        {
                            TotalEstimatedDistance = result.rows.FirstOrDefault().elements.FirstOrDefault().distance.value;
                            TotalEstimatedTime = result.rows.FirstOrDefault().elements.FirstOrDefault().duration.value;
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            EstimatedValueDTO estimatedValueDTO = new EstimatedValueDTO()
            {
                TotalEstimatedDistance = TotalEstimatedDistance,
                TotalEstimatedTime = TotalEstimatedTime
            };
            return estimatedValueDTO;
        }
        public static async Task<DistanceTextDto> GetEstimatedDataBetweenTwoLocationsText(double Latitude1, double Longitude1, double Latitude2, double Longitude2)
        {
            string GoogleDistanceKey = ConfigurationManager.AppSettings["GOOGLE_DISTANCE_MATRIX_KEY"];
            string apiUrl = $"https://maps.googleapis.com/maps/api/distancematrix/json?origins={Latitude1},{Longitude1}&destinations={Latitude2},{Longitude2}&key={GoogleDistanceKey}";
            string TotalEstimatedDistance = "";
            string TotalEstimatedTime = "";
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<DistanceMatrixDTO>(data);
                        if (result != null && result.rows != null && result.rows.Count > 0 && !result.rows.Any(x => x == null) && !result.rows.FirstOrDefault().elements.Any(x => x.distance == null) && !result.rows.FirstOrDefault().elements.Any(x => x.duration == null))
                        {
                            TotalEstimatedDistance = result.rows.FirstOrDefault().elements.FirstOrDefault().distance.text;
                            TotalEstimatedTime = result.rows.FirstOrDefault().elements.FirstOrDefault().duration.text;
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            DistanceTextDto estimatedValueDTO = new DistanceTextDto()
            {
                TotalEstimatedDistance = TotalEstimatedDistance,
                TotalEstimatedTime = TotalEstimatedTime
            };
            return estimatedValueDTO;
        }
        public static double ToRadians(this double val)
        {
            return (Math.PI / 180) * val;
        }
    }
}