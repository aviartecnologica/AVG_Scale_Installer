using System;
using Android.Content;
using Android.Net.Wifi;

namespace AVG_Scale_Installer.Tools
{
    public class WifiReceiver : BroadcastReceiver
    {
        Action ScanSuccess, ScanFailure;

        public WifiReceiver(Action success, Action failure)
        {
            ScanSuccess = success;
            ScanFailure = failure;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            bool success = intent.GetBooleanExtra(WifiManager.ExtraResultsUpdated, false);

            if (success)
            {
                ScanSuccess();
            }
            else
            {
                ScanFailure();
            }
        }
    }
}
