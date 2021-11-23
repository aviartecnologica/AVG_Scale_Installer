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
using System.Threading.Tasks;

namespace AVG_Scale_Installer
{
    public class RequestAPI
    {
        public static async Task<LoginResponse> DoAPIAuthentication(string _login, string _password)
        {
            string uri = "https://dashboards.aviagensau.com/api/1.0/account/authenticate";
            var jsonString = System.Text.Json.JsonSerializer.Serialize(new { login = _login, password = _password });
            var client = new HttpClient();
            var response = await client.PostAsync(uri, new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json"));

            string responseBody = await response.Content.ReadAsStringAsync();
            LoginResponse res = System.Text.Json.JsonSerializer.Deserialize<LoginResponse>(responseBody);
            return res;
        }

        public static async Task<EPerson> GetPerson(int userId, string token)
        {
            string uri = "https://dashboards.aviagensau.com/api/1.0/people?userId=" + userId;
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                EPerson person = System.Text.Json.JsonSerializer.Deserialize<EPerson>(responseBody);
                return person;
            }
            return null;
        }

        public static async Task<List<ECenter>> GetAllowedCenters(int personId, string token)
        {
            string uri = $"https://dashboards.aviagensau.com/api/1.0/people/{personId}/allowedcenters";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                List<ECenter> results = System.Text.Json.JsonSerializer.Deserialize<List<ECenter>>(responseBody);
                return results;
            }
            return null;
        }

        public static async Task<EPerson> GetRoom( string token)
        {
            string uri = "https://dashboards.aviagensau.com/api/1.0/centers/2/rooms";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                EPerson person = System.Text.Json.JsonSerializer.Deserialize<EPerson>(responseBody);
                return person;
            }
            return null;
        }
    }
}