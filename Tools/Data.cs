using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AVG_Scale_Installer.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AVG_Scale_Installer.Tools
{
    public static class Data
    {
        public static Android.Support.V4.App.FragmentManager CurrentFM;

        public static string WifiFilter = "";

        public static Network SelectedNetwork;
    }
}