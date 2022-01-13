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

        public static string Server;

        public static string WifiFilterScale = "exit-trq-";

        public static string WifiFilterBlackbox = "exit-trq-";

        public static string WifiPasswordScale = "trq_aviartec";

        public static string WifiPasswordBlackbox = "trq_aviartec";

        public static ECenter CurrentCenter;
    }
}