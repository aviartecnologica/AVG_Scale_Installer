using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AVG_access_data;
using AVG_Scale_Installer.Tools;
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
        private static string Token;

        public static async Task RefreshToken()
        {
            string uri = $"{Data.MyAddress.ToHttp()}/api/1.0/account/authenticate";
            string credentials = JsonSerializer.Serialize(new { login = "aviartec@aviartec.com", password = "qzlQoRutR0oDPWZPNQPs7AxqdG" });
            using(var client = new HttpClient())
            {
                try
                {
                    var response = await client.PostAsync(uri, new StringContent(credentials, Encoding.UTF8, "application/json"));
                    if (response.IsSuccessStatusCode)
                    {
                        string sesion = await response.Content.ReadAsStringAsync();
                        Sesion currentSesion = JsonSerializer.Deserialize<Sesion>(sesion);
                        Token = currentSesion.token;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("API", ex.Message);
                }
            }
        }

        public static async Task<ECenter> GetCurrentCenter()
        {
            string uri = $"{Data.MyAddress.ToHttp()}/api/1.0/centers/current";

            using(var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                try
                {
                    var response = await client.GetAsync(uri);
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        await RefreshToken();
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                        response = await client.GetAsync(uri);
                    }
                    if (response.IsSuccessStatusCode)
                    {
                        string centerJson = await response.Content.ReadAsStringAsync();
                        return JsonSerializer.Deserialize<ECenter>(centerJson);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("API", ex.Message);
                }
            }
            return null;
        }

        public static async Task<List<ERoom>> GetRooms( int centerId)
        {
            string uri = $"{Data.MyAddress.ToHttp()}/api/1.0/centers/{centerId}/rooms?type=1";
            using(var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                try
                {
                    var response = await client.GetAsync(uri);
                    if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        await RefreshToken();
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                        response = await client.GetAsync(uri);
                    }
                    if (response.IsSuccessStatusCode)
                    {
                        string roomsJson = await response.Content.ReadAsStringAsync();
                        List<ERoom> rooms = JsonSerializer.Deserialize<List<ERoom>>(roomsJson);
                        return rooms;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("API", ex.Message);
                }
            }
            return null;
        }

        public static async Task<List<ELitter>> GetLitters(int centerId, int number)
        {
            string uri = $"{Data.MyAddress.ToHttp()}/api/1.0/centers/{centerId}/rooms/1_{number}/litters";
            using(var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                try
                {
                    var response = await client.GetAsync(uri);
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        await RefreshToken();
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                        response = await client.GetAsync(uri);
                    }
                    if (response.IsSuccessStatusCode)
                    {
                        string littersJson = await response.Content.ReadAsStringAsync();
                        List<ELitter> litters = JsonSerializer.Deserialize<List<ELitter>>(littersJson);
                        return litters;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("API", ex.Message);
                }
            }
            return null;
        }

        public static async Task<List<EBlackBoxesScale>> GetScales(int centerId, int number, int department, int lot)
        {
            string uri = $"{Data.MyAddress.ToHttp()}/api/1.0/centers/{centerId}/rooms/1_{number}/litters/{department}_{lot}/scales";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                try
                {
                    var response = await client.GetAsync(uri);
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        await RefreshToken();
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                        response = await client.GetAsync(uri);
                    }
                    if (response.IsSuccessStatusCode)
                    {
                        string scalesJson = await response.Content.ReadAsStringAsync();
                        List<EBlackBoxesScale> scales = JsonSerializer.Deserialize<List<EBlackBoxesScale>>(scalesJson);
                        return scales;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("API", ex.Message);
                }
            }
            return null;
        }

        public static async Task<List<EBlackBoxesBlackBox>> GetBlackboxes(int centerId, int number)
        {
            string uri = $"{Data.MyAddress.ToHttp()}/api/1.0/centers/{centerId}/rooms/1_{number}/blackboxes";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                try
                {
                    var response = await client.GetAsync(uri);
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        await RefreshToken();
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                        response = await client.GetAsync(uri);
                    }
                    if (response.IsSuccessStatusCode)
                    {
                        string blackboxesJson = await response.Content.ReadAsStringAsync();
                        List<EBlackBoxesBlackBox> blackboxes = JsonSerializer.Deserialize<List<EBlackBoxesBlackBox>>(blackboxesJson);
                        return blackboxes;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("API", ex.Message);
                }
            }
            return null;
        }

        public static async Task<bool> InsertScaleLocation(EBlackBoxesLocation location)
        {
            bool result = false;
            string uri = $"{Data.MyAddress.ToHttp()}/api/1.0/blackboxdevices/scales/{location.device.mac}/locations";
            var jsonLocation = JsonSerializer.Serialize(new List<EBlackBoxesLocation>() { location });
            using(var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                try
                {
                    var response = await client.PostAsync(uri, new StringContent(jsonLocation, Encoding.UTF8, "application/json"));
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        await RefreshToken();
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                        response = await client.PostAsync(uri, new StringContent(jsonLocation, Encoding.UTF8, "application/json"));
                    }
                    result = response.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    result = false;
                    Log.Error("API", ex.Message);
                }
            }
            return result;
        }

        public static async Task<EBlackBoxesScale> GetScaleByMac(string mac)
        {
            string uri = $"{Data.MyAddress.ToHttp()}/api/1.0/blackboxdevices/scales/{mac}";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                try
                {
                    var response = await client.GetAsync(uri);
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        await RefreshToken();
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                        response = await client.GetAsync(uri);
                    }
                    if (response.IsSuccessStatusCode)
                    {
                        string scaleJson = await response.Content.ReadAsStringAsync();
                        EBlackBoxesScale scale = JsonSerializer.Deserialize<EBlackBoxesScale>(scaleJson);
                        return scale;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("API", ex.Message);
                }
                return null;
            }
        }

        public static async Task<EBlackBoxesBlackBox> GetBlackBoxByMac(string mac)
        {
            string uri = $"{Data.MyAddress.ToHttp()}/api/1.0/blackboxdevices/blackboxes/{mac}";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                try
                {
                    var response = await client.GetAsync(uri);
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        await RefreshToken();
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                        response = await client.GetAsync(uri);
                    }
                    if (response.IsSuccessStatusCode)
                    {
                        string blackboxJson = await response.Content.ReadAsStringAsync();
                        EBlackBoxesBlackBox blackbox = JsonSerializer.Deserialize<EBlackBoxesBlackBox>(blackboxJson);
                        return blackbox;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("API", ex.Message);
                }
                return null;
            }
        }

        public static async Task<bool> UpdateScale(EBlackBoxesScale scale)
        {
            bool result = false;
            string uri = $"{Data.MyAddress.ToHttp()}/api/1.0/blackboxdevices/scales/{scale.mac}";
            var jsonScale = JsonSerializer.Serialize(scale);
            using(var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                try
                {
                    var response = await client.PutAsync(uri, new StringContent(jsonScale, Encoding.UTF8, "application/json"));
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        await RefreshToken();
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                        response = await client.PutAsync(uri, new StringContent(jsonScale, Encoding.UTF8, "application/json"));
                    }
                    result = response.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    result = false;
                    Log.Error("API", ex.Message);
                }
            }
            return result;
        }

        public static async Task<bool> UpdateBlackBox(EBlackBoxesBlackBox blackbox)
        {
            bool result = false;
            string uri = $"{Data.MyAddress.ToHttp()}/api/1.0/blackboxdevices/blackboxes/{blackbox.mac}";
            var jsonBlackbox = JsonSerializer.Serialize(blackbox);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                try
                {
                    var response = await client.PutAsync(uri, new StringContent(jsonBlackbox, Encoding.UTF8, "application/json"));
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        await RefreshToken();
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                        response = await client.PutAsync(uri, new StringContent(jsonBlackbox, Encoding.UTF8, "application/json"));
                    }
                    result = response.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    result = false;
                    Log.Error("API", ex.Message);
                }
            }
            return result;
        }

    }

    public class Sesion
    {
        public bool success { get; set; }
        public string user { get; set; }
        public string email { get; set; }
        public int id { get; set; }
        public List<string> roles { get; set; }
        public string token { get; set; }
        public DateTime? expiration { get; set; }
    }
}