using Android.App;
using Android.Content;
using Android.Net.Wifi;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AVG_Scale_Installer.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AVG_Scale_Installer.Tools
{
    public class WifiMonitor : BroadcastReceiver
    {
        private WifiManager myWifiManager;
        private RecyclerView WifiRecycler;
        private SwipeRefreshLayout WifiSwipe;
        private SwipeRefreshLayout EmptySwipe;

        public WifiMonitor(WifiManager wifiManager, RecyclerView wifiRecycler, SwipeRefreshLayout wifiSwipe, SwipeRefreshLayout emptySwipe)
        {
            myWifiManager = wifiManager;
            WifiRecycler = wifiRecycler;
            WifiSwipe = wifiSwipe;
            EmptySwipe = emptySwipe;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            string action = intent.Action;
            if (WifiManager.ScanResultsAvailableAction.Equals(action))
            {
                var networks = new List<Network>(myWifiManager.ScanResults.Select(x => new Network(x, false)));
                var adapter = new WifisAdapter(networks);
                adapter.ItemClick += (s, pos) =>
                {
                    //Click listener
                    if(Data.SelectedNetwork == null)
                    {
                        //Nada seleccionado
                        Data.SelectedNetwork = networks[pos];
                        networks[pos].Selected = true;
                        adapter.NotifyItemChanged(pos);
                    }
                    else
                    {
                        if(Data.SelectedNetwork == networks[pos])
                        {
                            //Click en el que ya está seleccionado (Deseleccionar)
                            Data.SelectedNetwork.Selected = false;
                            Data.SelectedNetwork = null;
                            adapter.NotifyItemChanged(pos);
                        }
                        else
                        {
                            //Click en cualquier otro (Selección del nuevo y deselección del antiguo)
                            int prevPos = networks.FindIndex(x => x == Data.SelectedNetwork);
                            Data.SelectedNetwork.Selected = false;
                            adapter.NotifyItemChanged(prevPos);

                            Data.SelectedNetwork = networks[pos];
                            Data.SelectedNetwork.Selected = true;
                            adapter.NotifyItemChanged(pos);
                        }
                    }
                };
                WifiRecycler.SetAdapter(adapter);

                if(networks.Where(x => x.ScanResult.Ssid.StartsWith(Data.WifiFilter)).ToList().Count == 0)
                {
                    EmptySwipe.Visibility = ViewStates.Visible;
                    WifiSwipe.Visibility = ViewStates.Gone;
                }
                else
                {
                    EmptySwipe.Visibility = ViewStates.Gone;
                    WifiSwipe.Visibility = ViewStates.Visible;
                }
            }
        }
    }
}