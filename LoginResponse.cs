using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AVG_Scale_Installer
{
    public class LoginResponse
    {
        public bool success { get; set; }
        public string user { get; set; }
        public string email { get; set; }
        public int? id { get; set; }
        public List<string> roles { get; set; }
        public string token { get; set; }
        public DateTime? expiration { get; set; }
    }
}