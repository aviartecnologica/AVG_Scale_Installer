using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AVG_access_data;
using AVG_Scale_Installer.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AVG_Scale_Installer.Tools
{
    public static class Data
    {
        public static AndroidX.Fragment.App.FragmentManager CurrentFM;

        public static Address MyAddress;

        public static string WifiFilterScale = "scale-";

        public static string WifiFilterBlackbox = "blackbox-";

        public static string WifiPasswordScale = "avg_aviartec";

        public static string WifiPasswordBlackbox = "avg_aviartec";

        public static ECenter CurrentCenter;
    }

    public class Address
    {
        public string Server { get; set; }
        public string Port { get; set; }

        public Address(string _ip)
        {
            string[] ip = _ip.Split(":");
            if (ip.Length == 2)
            {
                Server = ip[0];
                Port = ip[1];
            }
            else
            {
                Server = "";
                Port = "";
            }
        }

        public string ToHttp()
        {
            var type = Uri.CheckHostName(Server);
            string http;
            switch (type)
            {
                case UriHostNameType.Dns:
                    http = "https";
                    break;
                case UriHostNameType.IPv4:
                    http = "http";
                    break;
                default:
                    http = "http";
                    break;
            }

            return $"{http}://{Server}:{Port}";
        }

        public string ToMqtt()
        {
            return Server;
        }
    }
}