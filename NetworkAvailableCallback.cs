using System;
using Android.Content;
using Android.Net;

namespace AVG_Scale_Installer
{
    public class NetworkAvailableCallback : ConnectivityManager.NetworkCallback
    {
        private ConnectivityManager ConnManager;

        public NetworkAvailableCallback(Context context)
        {
            ConnManager = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
        }

        public override void OnAvailable(Network network)
        {
            ConnManager.BindProcessToNetwork(network);
        }
    }
}
