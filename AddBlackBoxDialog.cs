
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using AVG_access_data;
using AVG_Scale_Installer.Adapters;
using AVG_Scale_Installer.Tools;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace AVG_Scale_Installer
{
    public class AddBlackBoxDialog : DialogFragment
    {
        private List<ERoom> RoomsList;
        private WifiManager myWifiManager;
        private LinearLayout ConfigModeLayout;
        private WebView ConfigModeInfo;
        private Button ConfigModeContinue;
        private LinearLayout WifiSelectionLayout;
        private SwipeRefreshLayout WifiSwipe;
        private RecyclerView WifiRecycler;
        private SwipeRefreshLayout WifiEmptySwipe;
        private Button WifiIdentify;
        private Button WifiContinue;
        private WifiNetwork SelectedNetwork;
        private LinearLayout RoomSelectionLayout;
        private SwipeRefreshLayout RoomSwipe;
        private RecyclerView RoomRecycler;
        private SwipeRefreshLayout RoomEmptySwipe;
        private Button RoomContinue;
        private Room SelectedRoom;
        private LinearLayout NameSelectionLayout;
        private EditText NameInput;
        private Button NameContinue;
        private LinearLayout FinishLayout;
        private Button FinishButton;
        private WifiReceiver myWifiReceiver;
        private List<WifiNetwork> Networks;
        private WifisAdapter NetworkAdapter;
        private WifiInfo DefaultNetwork;
        private BlinkCallback myBlinkCallback;
        private WifiCallback myWifiCallback;
        private List<Room> Houses;
        private RoomsAdapter RoomAdapter;

        public override void OnStart()
        {
            base.OnStart();

            Android.App.Dialog dialog = Dialog;
            if(dialog != null)
            {
                int width = ViewGroup.LayoutParams.MatchParent;
                int height = ViewGroup.LayoutParams.MatchParent;
                dialog.Window.SetLayout(width, height);
            }
        }

        public async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(DialogFragment.StyleNormal, Resource.Style.AppTheme_FullScreenDialog);

            RoomsList = await RequestAPI.GetRooms(Data.CurrentCenter.idCenter);
            if(RoomsList == null)
            {
                Toast.MakeText(Activity, "ERROR", ToastLength.Short).Show();
                Dismiss();
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.AddBlackboxDialog, container, false);

            //SetUp WifiMonitor
            myWifiManager = (WifiManager)Context.GetSystemService(Context.WifiService);
            if (!myWifiManager.IsWifiEnabled)
            {
                Toast.MakeText(Context, Resource.String.wifi_disabled, ToastLength.Short).Show();
            }

            //Toolbar
            Toolbar toolbar = view.FindViewById<Toolbar>(Resource.Id.AddBlackboxDialogToolbar);
            toolbar.SetTitle(Resource.String.add_blackbox);
            toolbar.SetNavigationIcon(Resource.Drawable.clear);
            toolbar.NavigationClick += Toolbar_NavigationClick;

            //Pantalla led parpadeando
            ConfigModeLayout = view.FindViewById<LinearLayout>(Resource.Id.AddBlackboxDialogConfigMode);
            ConfigModeInfo = view.FindViewById<WebView>(Resource.Id.AddBlackboxDialogConfigModeInfoText);
            ConfigModeContinue = view.FindViewById<Button>(Resource.Id.AddBlackboxDialogConfigModeContinueButton);
            string info = "<html><body text='white' style='text-align:justify;'>" + Resources.GetText(Resource.String.config_mode_info_blackbox) + "</body></html>";
            ConfigModeInfo.SetBackgroundColor(Color.Transparent);
            ConfigModeInfo.LoadData(info, "text/html; charset=utf-8", "UTF-8");
            ConfigModeContinue.Click += ConfigModeContinue_Click;

            //Pantalla seleccionar báscula (wifi)
            WifiSelectionLayout = view.FindViewById<LinearLayout>(Resource.Id.AddBlackboxDialogWifiSelection);
            WifiSwipe = view.FindViewById<SwipeRefreshLayout>(Resource.Id.AddBlackboxDialogWifiSwipeContainer);
            WifiRecycler = view.FindViewById<RecyclerView>(Resource.Id.AddBlackboxDialogWifiRecycler);
            WifiEmptySwipe = view.FindViewById<SwipeRefreshLayout>(Resource.Id.AddBlackboxDialogWifiEmptySwipeContainer);
            WifiIdentify = view.FindViewById<Button>(Resource.Id.AddBlackboxDialogWifiIdentifyButton);
            WifiContinue = view.FindViewById<Button>(Resource.Id.AddBlackboxDialogWifiContinueButton);
            WifiRecycler.SetLayoutManager(new LinearLayoutManager(Context));
            WifiSwipe.SetProgressBackgroundColorSchemeResource(Resource.Color.lighter);
            WifiEmptySwipe.SetProgressBackgroundColorSchemeResource(Resource.Color.lighter);
            WifiSwipe.SetColorSchemeResources(Resource.Color.colorAccent);
            WifiEmptySwipe.SetColorSchemeResources(Resource.Color.colorAccent);
            WifiSwipe.Refresh += Wifi_Refresh;
            WifiEmptySwipe.Refresh += Wifi_Refresh;
            WifiIdentify.Click += WifiIdentify_Click;
            WifiContinue.Click += WifiContinue_Click;
            SelectedNetwork = null;

            //Pantalla seleccionar nave
            RoomSelectionLayout = view.FindViewById<LinearLayout>(Resource.Id.AddBlackboxDialogRoomSelection);
            RoomSwipe = view.FindViewById<SwipeRefreshLayout>(Resource.Id.AddBlackboxDialogWRoomSwipeContainer);
            RoomRecycler = view.FindViewById<RecyclerView>(Resource.Id.AddBlackboxDialogRoomRecycler);
            RoomEmptySwipe = view.FindViewById<SwipeRefreshLayout>(Resource.Id.AddBlackboxDialogRoomEmptySwipeContainer);
            RoomContinue = view.FindViewById<Button>(Resource.Id.AddBlackboxDialogRoomContinueButton);
            RoomRecycler.SetLayoutManager(new LinearLayoutManager(Context));
            RoomSwipe.SetProgressBackgroundColorSchemeResource(Resource.Color.lighter);
            RoomEmptySwipe.SetProgressBackgroundColorSchemeResource(Resource.Color.lighter);
            RoomSwipe.SetColorSchemeResources(Resource.Color.colorAccent);
            RoomEmptySwipe.SetColorSchemeResources(Resource.Color.colorAccent);
            RoomSwipe.Refresh += Room_Refresh;
            RoomEmptySwipe.Refresh += Room_Refresh;
            RoomContinue.Click += RoomContinue_Click;
            SelectedRoom = null;

            //Pantalla establecer nombre
            NameSelectionLayout = view.FindViewById<LinearLayout>(Resource.Id.AddBlackboxDialogNameSelection);
            NameInput = view.FindViewById<EditText>(Resource.Id.AddBlackboxDialogNameEditText);
            NameContinue = view.FindViewById<Button>(Resource.Id.AddBlackboxDialogNameContinue);
            NameInput.AfterTextChanged += CheckVoid;
            NameContinue.Click += NameContinue_Click;

            //Pantalla fin
            FinishLayout = view.FindViewById<LinearLayout>(Resource.Id.AddBlackboxDialogFinishLayout);
            FinishButton = view.FindViewById<Button>(Resource.Id.AddBlackboxDialogFinishButton);
            FinishButton.Click += FinishButton_Click;

            //Workflow
            view.FindViewById<LinearLayout>(Resource.Id.AddBlackboxDialogLayout).Touch += delegate
            {
                Functions.HideKeyboard(Activity);
            };

            return view;
        }

        #region Workflow

        public override void OnResume()
        {
            base.OnResume();

            myWifiReceiver = new WifiReceiver(OnSuccessScan, OnFailureScan);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            try
            {
                Context.UnregisterReceiver(myWifiReceiver);
            }
            catch (Exception) { }
        }

        private void CheckVoid(object sender, AfterTextChangedEventArgs e)
        {
            EditText input = (EditText)sender;
            if (input.Text.Length == 0)
            {
                NameContinue.Enabled = false;
            }
            else
            {
                NameContinue.Enabled = true;
            }
        }

        private void Toolbar_NavigationClick(object sender, Toolbar.NavigationClickEventArgs e)
        {
            Dismiss();
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            if(Build.VERSION.SdkInt >= BuildVersionCodes.M
                && Context.CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) != (int)Permission.Granted
                && Context.CheckSelfPermission(Manifest.Permission.AccessFineLocation) != (int)Permission.Granted)
            {
                RequestPermissions(new string[] { Manifest.Permission.AccessFineLocation, Manifest.Permission.AccessCoarseLocation }, 1);
            }
        }

        #endregion

        #region Config

        private void ConfigModeContinue_Click(object sender, EventArgs e)
        {
            ConfigModeLayout.Visibility = ViewStates.Gone;
            WifiSelectionLayout.Visibility = ViewStates.Visible;
            RoomSelectionLayout.Visibility = ViewStates.Gone;
            NameSelectionLayout.Visibility = ViewStates.Gone;
            FinishLayout.Visibility = ViewStates.Gone;

            WifiSwipe.Refreshing = true;
            WifiEmptySwipe.Refreshing = true;
            WifiSwipe.Visibility = ViewStates.Visible;
            WifiEmptySwipe.Visibility = ViewStates.Gone;

            IntentFilter intentFilter = new IntentFilter();
            intentFilter.AddAction(WifiManager.ScanResultsAvailableAction);
            Context.RegisterReceiver(myWifiReceiver, intentFilter);
            bool success = myWifiManager.StartScan();
            if (!success)
            {
                OnFailureScan();
            }
        }

        #endregion

        #region Wifi

        private void OnSuccessScan()
        {
            Context.UnregisterReceiver(myWifiReceiver);

            SelectedNetwork = null;
            WifiIdentify.Enabled = false;
            WifiContinue.Enabled = false;

            Networks = new List<WifiNetwork>(myWifiManager.ScanResults.Select(x => new WifiNetwork(x, false)));
            Networks = Networks.Where(x => x.ScanResult.Ssid.StartsWith(Data.WifiFilterBlackbox)).ToList();
            NetworkAdapter = new WifisAdapter(Networks);
            NetworkAdapter.ItemClick += Network_Click;
            WifiRecycler.SetAdapter(NetworkAdapter);

            if (Networks.Count == 0)
            {
                WifiEmptySwipe.Visibility = ViewStates.Visible;
                WifiSwipe.Visibility = ViewStates.Gone;
                WifiEmptySwipe.Refreshing = false;
            }
            else
            {
                WifiEmptySwipe.Visibility = ViewStates.Gone;
                WifiSwipe.Visibility = ViewStates.Visible;
                WifiSwipe.Refreshing = false;
            }
        }

        private void OnFailureScan()
        {
            Toast.MakeText(Context, "Scan throtled", ToastLength.Short).Show();
            OnSuccessScan();
        }

        private void Network_Click(object sender, int pos)
        {
            //Click listener
            if (SelectedNetwork == null)
            {
                //Nada seleccionado
                SelectedNetwork = Networks[pos];
                Networks[pos].Selected = true;
                NetworkAdapter.NotifyItemChanged(pos);
            }
            else
            {
                if (SelectedNetwork == Networks[pos])
                {
                    //Click en el que ya está seleccionado (Deseleccionar)
                    SelectedNetwork.Selected = false;
                    SelectedNetwork = null;
                    NetworkAdapter.NotifyItemChanged(pos);
                }
                else
                {
                    //Click en cualquier otro (Selección del nuevo y deselección del antiguo)
                    int prevPos = Networks.FindIndex(x => x == SelectedNetwork);
                    SelectedNetwork.Selected = false;
                    NetworkAdapter.NotifyItemChanged(prevPos);

                    SelectedNetwork = Networks[pos];
                    SelectedNetwork.Selected = true;
                    NetworkAdapter.NotifyItemChanged(pos);
                }
            }
            NotifyNetworkSelection();
        }

        public void NotifyNetworkSelection()
        {
            if (SelectedNetwork != null)
            {
                WifiIdentify.Enabled = true;
                WifiContinue.Enabled = true;
            }
            else
            {
                WifiIdentify.Enabled = false;
                WifiContinue.Enabled = false;
            }
        }

        private void Wifi_Refresh(object sender, EventArgs e)
        {
            IntentFilter intentFilter = new IntentFilter();
            intentFilter.AddAction(WifiManager.ScanResultsAvailableAction);
            Context.RegisterReceiver(myWifiReceiver, intentFilter);

            bool success = myWifiManager.StartScan();
            if (!success)
            {
                OnFailureScan();
            }
        }

        private async void WifiIdentify_Click(object sender, EventArgs e)
        {
            DefaultNetwork = myWifiManager.ConnectionInfo;
            Functions.Loading(true);

            NetworkSpecifier specifier = new WifiNetworkSpecifier.Builder()
                .SetSsid(SelectedNetwork.ScanResult.Ssid)
                .SetWpa2Passphrase(Data.WifiPasswordBlackbox)
                .Build();

            NetworkRequest request =
                new NetworkRequest.Builder()
                .AddTransportType(TransportType.Wifi)
                .RemoveCapability(NetCapability.Internet)
                .SetNetworkSpecifier(specifier)
                .Build();

            ConnectivityManager connectivityManager = (ConnectivityManager)
                Context.GetSystemService(Context.ConnectivityService);

            myBlinkCallback = new BlinkCallback(connectivityManager, BlinkFinished);

            connectivityManager.RequestNetwork(request, myBlinkCallback);
        }

        private void BlinkFinished(bool success)
        {
            WifiInfo info = myWifiManager.ConnectionInfo;
            DateTime chagingWifi = DateTime.Now;
            while (info.SSID != DefaultNetwork.SSID)
            {
                info = myWifiManager.ConnectionInfo;
                if (DateTime.Now.Subtract(chagingWifi).Seconds >= 30)
                {
                    Functions.Loading(false);
                    Dismiss();
                    break;
                }
            }
            Functions.Loading(false);
            if (!success)
            {
                Activity.RunOnUiThread(() => Toast.MakeText(Context, Resource.String.wifi_denied, ToastLength.Short).Show());
            }
        }

        private void WifiContinue_Click(object sender, EventArgs e)
        {
            DefaultNetwork = myWifiManager.ConnectionInfo;
            Functions.Loading(true);

            NetworkSpecifier specifier = new WifiNetworkSpecifier.Builder()
                .SetSsid(SelectedNetwork.ScanResult.Ssid)
                .SetWpa2Passphrase(Data.WifiPasswordBlackbox)
                .Build();

            NetworkRequest request = new NetworkRequest.Builder()
                .AddTransportType(TransportType.Wifi)
                .RemoveCapability(NetCapability.Internet)
                .SetNetworkSpecifier(specifier)
                .Build();

            ConnectivityManager connectivityManager = (ConnectivityManager)
                Context.GetSystemService(Context.ConnectivityService);

            myWifiCallback = new WifiCallback(connectivityManager, WifiConnected);

            connectivityManager.RequestNetwork(request, myWifiCallback);
        }

        private void WifiConnected(bool success, ConnectivityManager cm)
        {
            if (!success)
            {
                Activity.RunOnUiThread(() =>
                {
                    Toast.MakeText(Context, Resource.String.wifi_denied, ToastLength.Short).Show();
                    Dismiss();
                });
            }
            else
            {
                Activity.RunOnUiThread(() =>
                {
                    ConfigModeLayout.Visibility = ViewStates.Gone;
                    WifiSelectionLayout.Visibility = ViewStates.Gone;
                    RoomSelectionLayout.Visibility = ViewStates.Visible;
                    NameSelectionLayout.Visibility = ViewStates.Gone;
                    FinishLayout.Visibility = ViewStates.Gone;

                    RoomSwipe.Refreshing = true;
                    RoomEmptySwipe.Refreshing = true;
                    RoomSwipe.Visibility = ViewStates.Visible;
                    RoomEmptySwipe.Visibility = ViewStates.Gone;

                    OnRoomsLoaded();
                });
            }
        }

        #endregion

        #region Room

        private void OnRoomsLoaded()
        {
            SelectedRoom = null;
            RoomContinue.Enabled = false;

            Houses = new List<Room>(RoomsList.Select(x => new Room(x, false)));
            RoomAdapter = new RoomsAdapter(Houses);
            RoomAdapter.ItemClick += Room_Click;
            RoomRecycler.SetAdapter(RoomAdapter);

            if (Houses.Count == 0)
            {
                RoomEmptySwipe.Visibility = ViewStates.Visible;
                RoomSwipe.Visibility = ViewStates.Gone;
                RoomEmptySwipe.Refreshing = false;
            }
            else
            {
                RoomEmptySwipe.Visibility = ViewStates.Gone;
                RoomSwipe.Visibility = ViewStates.Visible;
                RoomSwipe.Refreshing = false;
            }
        }

        private void Room_Click(object sender, int pos)
        {
            //Click listener
            if (SelectedRoom == null)
            {
                //Nada seleccionado
                SelectedRoom = Houses[pos];
                Houses[pos].Selected = true;
                RoomAdapter.NotifyItemChanged(pos);
            }
            else
            {
                if (SelectedRoom == Houses[pos])
                {
                    //Click en el que ya está seleccionado (Deseleccionar)
                    SelectedRoom.Selected = false;
                    SelectedRoom = null;
                    RoomAdapter.NotifyItemChanged(pos);
                }
                else
                {
                    //Click en cualquier otro (Selección del nuevo y deselección del antiguo)
                    int prevPos = Houses.FindIndex(x => x == SelectedRoom);
                    SelectedRoom.Selected = false;
                    RoomAdapter.NotifyItemChanged(prevPos);

                    SelectedRoom = Houses[pos];
                    SelectedRoom.Selected = true;
                    RoomAdapter.NotifyItemChanged(pos);
                }
            }
            NotifyRoomSelection();
        }

        private void NotifyRoomSelection()
        {
            if (SelectedRoom != null)
            {
                RoomContinue.Enabled = true;
            }
            else
            {
                RoomContinue.Enabled = false;
            }
        }

        private void Room_Refresh(object sender, EventArgs e)
        {
            OnRoomsLoaded();
        }

        private void RoomContinue_Click(object sender, EventArgs e)
        {
            ConfigModeLayout.Visibility = ViewStates.Gone;
            WifiSelectionLayout.Visibility = ViewStates.Gone;
            RoomSelectionLayout.Visibility = ViewStates.Gone;
            NameSelectionLayout.Visibility = ViewStates.Visible;
            FinishLayout.Visibility = ViewStates.Gone;
        }

        #endregion

        #region Name

        private void NameContinue_Click(object sender, EventArgs e)
        {
            Functions.HideKeyboard(Activity);
            NameInput.ClearFocus();

            //ADD ALL
            string uri = "http://192.168.4.1/setup?";


            //Desconexión del punto de acceso
            ConnectivityManager connectivityManager = (ConnectivityManager)
                Context.GetSystemService(Context.ConnectivityService);
            connectivityManager.UnregisterNetworkCallback(myWifiCallback);
            connectivityManager.BindProcessToNetwork(null);


            ConfigModeLayout.Visibility = ViewStates.Gone;
            WifiSelectionLayout.Visibility = ViewStates.Gone;
            RoomSelectionLayout.Visibility = ViewStates.Gone;
            NameSelectionLayout.Visibility = ViewStates.Gone;
            FinishLayout.Visibility = ViewStates.Visible;
        }

        #endregion

        #region Finish

        private void FinishButton_Click(object sender, EventArgs e)
        {
            Dismiss();
        }

        #endregion
    }
}
