using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AVG_access_data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AVG_Scale_Installer
{
    public class RequestAPI
    {

        public static async Task<ECenter> GetCurrentCenter()
        {
            string uri = $"https://dashboards.aviagensau.com/api/1.0/centers/current";
            var client = new HttpClient();
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string body = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ECenter>(body);
            }
            return null;
        }

        public static async Task<List<ERoom>> GetRooms( int centerId)
        {
            string uri = $"https://dashboards.aviagensau.com/api/1.0/centers/{centerId}/rooms?type=1";
            var client = new HttpClient();
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                List<ERoom> list = JsonSerializer.Deserialize<List<ERoom>>(responseBody);
                return list;
            }
            return null;
        }

        public static async Task<List<ELitter>> GetLitters(int centerId, int number)
        {
            string uri = $"https://dashboards.aviagensau.com/api/1.0/centers/{centerId}/rooms/1_{number}/litters";
            var client = new HttpClient();
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                List<ELitter> list = JsonSerializer.Deserialize<List<ELitter>>(responseBody);
                return list;
            }
            return null;
        }

        public static async Task<List<EBlackBoxesScale>> GetScales(int centerId, int number, int department, int lot)
        {
            string uri = $"https://dashboards.aviagensau.com/api/1.0/centers/{centerId}/rooms/1_{number}/litters/{department}_{lot}/scales";
            var client = new HttpClient();
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                List<EBlackBoxesScale> list = JsonSerializer.Deserialize<List<EBlackBoxesScale>>(responseBody);
                return list;
            }
            return null;
        }

        public static async Task<List<EBlackBoxesBlackBox>> GetBlackboxes(int centerId, int number)
        {
            string uri = $"https://dashboards.aviagensau.com/api/1.0/centers/{centerId}/rooms/1_{number}/blackboxes";
            var client = new HttpClient();
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                List<EBlackBoxesBlackBox> list = JsonSerializer.Deserialize<List<EBlackBoxesBlackBox>>(responseBody);
                return list;
            }
            return null;
        }

    }
}