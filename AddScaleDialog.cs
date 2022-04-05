
using Android;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Preferences;
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
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace AVG_Scale_Installer
{
    public class AddScaleDialog : DialogFragment
    {
        private LinearLayout ConfigModeLayout;
        private WebView ConfigModeInfo;
        private Button ConfigModeContinue;
        private LinearLayout WifiSelectionLayout;
        private SwipeRefreshLayout WifiSwipe;
        private RecyclerView WifiRecycler;
        private SwipeRefreshLayout WifiEmptySwipe;
        private Button WifiIdentify;
        private Button WifiContinue;
        private WifiManager myWifiManager;
        private WifiReceiver myWifiReceiver;
        private WifiNetwork SelectedNetwork;
        private LinearLayout RoomSelectionLayout;
        private SwipeRefreshLayout RoomSwipe;
        private RecyclerView RoomRecycler;
        private SwipeRefreshLayout RoomEmptySwipe;
        private Button RoomContinue;
        private List<Department> Departments;
        private LittersAdapter LitterAdapter;
        private List<Room> Houses;
        private List<WifiNetwork> Networks;
        private WifisAdapter NetworkAdapter;
        private string SelectedMac;
        private WifiInfo DefaultNetwork;
        private BlinkCallback myBlinkCallback;
        private WifiCallback myWifiCallback;
        private RoomsAdapter RoomAdapter;
        private Room SelectedRoom;
        private LinearLayout LitterSelectionLayout;
        private SwipeRefreshLayout LitterSwipe;
        private RecyclerView LitterRecycler;
        private SwipeRefreshLayout LitterEmptySwipe;
        private Button LitterContinue;
        private Task GetRooms;
        private ISharedPreferences Prefs;
        private List<ERoom> RoomsList;
        private Department SelectedLitter;
        private LinearLayout NameSelectionLayout;
        private EditText NameInput;
        private Button NameContinue;
        private List<ELitter> LittersList;
        private LinearLayout PasswordSelectionLayout;
        private EditText PasswordInput;
        private Button PasswordContinue;
        private LinearLayout FinishLayout;
        private Button FinishButton;
        private string SelectedName;

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

            Prefs = PreferenceManager.GetDefaultSharedPreferences(Context);

            RoomsList = await RequestAPI.GetRooms(Data.CurrentCenter.idCenter);
            if(RoomsList != null)
            {
                LittersList = new List<ELitter>();
                foreach(ERoom r in RoomsList)
                {
                    var litters = await RequestAPI.GetLitters(Data.CurrentCenter.idCenter, r.number);
                    if (litters != null)
                        LittersList.AddRange(litters);
                }
            }
            else
            {
                Toast.MakeText(Activity, "ERROR", ToastLength.Short).Show();
                Dismiss();
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.AddScaleDialog, container, false);

            //SetUp WifiMonitor
            myWifiManager = (WifiManager)Context.GetSystemService(Context.WifiService);
            if (!myWifiManager.IsWifiEnabled)
            {
                Toast.MakeText(Context, Resource.String.wifi_disabled, ToastLength.Short).Show();
            }

            //Toolbar
            Toolbar toolbar = view.FindViewById<Toolbar>(Resource.Id.AddScaleDialogToolbar);
            toolbar.SetTitle(Resource.String.add_scale);
            toolbar.SetNavigationIcon(Resource.Drawable.clear);
            toolbar.NavigationClick += Toolbar_NavigationClick;

            //Pantalla led parpadeando
            ConfigModeLayout = view.FindViewById<LinearLayout>(Resource.Id.AddScaleDialogConfigMode);
            ConfigModeInfo = view.FindViewById<WebView>(Resource.Id.AddScaleDialogConfigModeInfoText);
            ConfigModeContinue = view.FindViewById<Button>(Resource.Id.AddScaleDialogConfigModeContinueButton);
            string info = "<html><body text='white' style='text-align:justify;'>" + Resources.GetText(Resource.String.config_mode_info_scale) + "</body></html>";
            ConfigModeInfo.SetBackgroundColor(Color.Transparent);
            ConfigModeInfo.LoadData(info, "text/html; charset=utf-8", "UTF-8");
            ConfigModeContinue.Click += ConfigModeContinue_Click;

            //Pantalla seleccionar báscula (wifi)
            WifiSelectionLayout = view.FindViewById<LinearLayout>(Resource.Id.AddScaleDialogWifiSelection);
            WifiSwipe = view.FindViewById<SwipeRefreshLayout>(Resource.Id.AddScaleDialogWifiSwipeContainer);
            WifiRecycler = view.FindViewById<RecyclerView>(Resource.Id.AddScaleDialogWifiRecycler);
            WifiEmptySwipe = view.FindViewById<SwipeRefreshLayout>(Resource.Id.AddScaleDialogWifiEmptySwipeContainer);
            WifiIdentify = view.FindViewById<Button>(Resource.Id.AddScaleDialogWifiIdentifyButton);
            WifiContinue = view.FindViewById<Button>(Resource.Id.AddScaleDialogWifiContinueButton);
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
            RoomSelectionLayout = view.FindViewById<LinearLayout>(Resource.Id.AddScaleDialogRoomSelection);
            RoomSwipe = view.FindViewById<SwipeRefreshLayout>(Resource.Id.AddScaleDialogWRoomSwipeContainer);
            RoomRecycler = view.FindViewById<RecyclerView>(Resource.Id.AddScaleDialogRoomRecycler);
            RoomEmptySwipe = view.FindViewById<SwipeRefreshLayout>(Resource.Id.AddScaleDialogRoomEmptySwipeContainer);
            RoomContinue = view.FindViewById<Button>(Resource.Id.AddScaleDialogRoomContinueButton);
            RoomRecycler.SetLayoutManager(new LinearLayoutManager(Context));
            RoomSwipe.SetProgressBackgroundColorSchemeResource(Resource.Color.lighter);
            RoomEmptySwipe.SetProgressBackgroundColorSchemeResource(Resource.Color.lighter);
            RoomSwipe.SetColorSchemeResources(Resource.Color.colorAccent);
            RoomEmptySwipe.SetColorSchemeResources(Resource.Color.colorAccent);
            RoomSwipe.Refresh += Room_Refresh;
            RoomEmptySwipe.Refresh += Room_Refresh;
            RoomContinue.Click += RoomContinue_Click;
            SelectedRoom = null;

            //Pantalla seleccionar departamento
            LitterSelectionLayout = view.FindViewById<LinearLayout>(Resource.Id.AddScaleDialogLitterSelection);
            LitterSwipe = view.FindViewById<SwipeRefreshLayout>(Resource.Id.AddScaleDialogLitterSwipeContainer);
            LitterRecycler = view.FindViewById<RecyclerView>(Resource.Id.AddScaleDialogLitterRecycler);
            LitterEmptySwipe = view.FindViewById<SwipeRefreshLayout>(Resource.Id.AddScaleDialogLitterEmptySwipeContainer);
            LitterContinue = view.FindViewById<Button>(Resource.Id.AddScaleDialogLitterContinueButton);
            LitterRecycler.SetLayoutManager(new LinearLayoutManager(Context));
            LitterSwipe.SetProgressBackgroundColorSchemeResource(Resource.Color.lighter);
            LitterEmptySwipe.SetProgressBackgroundColorSchemeResource(Resource.Color.lighter);
            LitterSwipe.SetColorSchemeResources(Resource.Color.colorAccent);
            LitterEmptySwipe.SetColorSchemeResources(Resource.Color.colorAccent);
            LitterSwipe.Refresh += Litter_Refresh;
            LitterEmptySwipe.Refresh += Litter_Refresh;
            LitterContinue.Click += LitterContinue_Click;
            SelectedLitter = null;

            //Pantalla establecer nombre
            NameSelectionLayout = view.FindViewById<LinearLayout>(Resource.Id.AddScaleDialogNameSelection);
            NameInput = view.FindViewById<EditText>(Resource.Id.AddScaleDialogNameEditText);
            NameContinue = view.FindViewById<Button>(Resource.Id.AddScaleDialogNameContinue);
            NameInput.AfterTextChanged += CheckVoid;
            NameContinue.Click += NameContinue_Click;

            //Pantalla establecer contraseña
            PasswordSelectionLayout = view.FindViewById<LinearLayout>(Resource.Id.AddScaleDialogPasswordSelection);
            PasswordInput = view.FindViewById<EditText>(Resource.Id.AddScaleDialogPasswordEditText);
            PasswordContinue = view.FindViewById<Button>(Resource.Id.AddScaleDialogPasswordContinue);
            PasswordInput.AfterTextChanged += CheckVoid;
            PasswordContinue.Click += PasswordContinue_Click;

            //Pantalla fin
            FinishLayout = view.FindViewById<LinearLayout>(Resource.Id.AddScaleDialogFinishLayout);
            FinishButton = view.FindViewById<Button>(Resource.Id.AddScaleDialogFinishButton);
            FinishButton.Click += FinishButton_Click;

            //Workflow
            view.FindViewById<LinearLayout>(Resource.Id.AddScaleDialogLayout).Touch += delegate
            {
                Functions.HideKeyboard(Activity);
            };

            return view;
        }


        #region Workflow

        public async override void OnResume()
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
            if(input.Text.Length == 0)
            {
                NameContinue.Enabled = false;
                PasswordContinue.Enabled = false;
            }
            else
            {
                NameContinue.Enabled = true;
                PasswordContinue.Enabled = true;
            }
        }
        
        private void Toolbar_NavigationClick(object sender, Toolbar.NavigationClickEventArgs e)
        {
            Dismiss();
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M &&
                Context.CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) != (int)Permission.Granted &&
                Context.CheckSelfPermission(Manifest.Permission.AccessFineLocation) != (int)Permission.Granted)
            {
                RequestPermissions(new string[] { Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation }, 1);
            }
        }

        #endregion

        #region Config

        private void ConfigModeContinue_Click(object sender, EventArgs e)
        {
            ConfigModeLayout.Visibility = ViewStates.Gone;
            WifiSelectionLayout.Visibility = ViewStates.Visible;
            RoomSelectionLayout.Visibility = ViewStates.Gone;
            LitterSelectionLayout.Visibility = ViewStates.Gone;
            NameSelectionLayout.Visibility = ViewStates.Gone;
            PasswordSelectionLayout.Visibility = ViewStates.Gone;
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
            Networks = Networks.Where(x => x.ScanResult.Ssid.StartsWith(Data.WifiFilterScale)).ToList();
            NetworkAdapter = new WifisAdapter(Networks);
            NetworkAdapter.ItemClick += Network_Click;
            WifiRecycler.SetAdapter(NetworkAdapter);

            if(Networks.Count == 0)
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
            if(SelectedNetwork == null)
            {
                //Nada seleccionado
                SelectedNetwork = Networks[pos];
                Networks[pos].Selected = true;
                NetworkAdapter.NotifyItemChanged(pos);
            }
            else
            {
                if(SelectedNetwork == Networks[pos])
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
            if(SelectedNetwork != null)
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
                .SetWpa2Passphrase(Data.WifiPasswordScale)
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
            while(info.SSID != DefaultNetwork.SSID)
            {
                info = myWifiManager.ConnectionInfo;
                if(DateTime.Now.Subtract(chagingWifi).Seconds >= 30)
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
            var macs = SelectedNetwork.ScanResult.Ssid.Split("-");
            SelectedMac = macs.Length == 2 ? macs[1] : "";
            DefaultNetwork = myWifiManager.ConnectionInfo;
            Functions.Loading(true);

            NetworkSpecifier specifier = new WifiNetworkSpecifier.Builder()
                .SetSsid(SelectedNetwork.ScanResult.Ssid)
                .SetWpa2Passphrase(Data.WifiPasswordScale)
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
            Functions.Loading(false);
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
                    LitterSelectionLayout.Visibility = ViewStates.Gone;
                    NameSelectionLayout.Visibility = ViewStates.Gone;
                    PasswordSelectionLayout.Visibility = ViewStates.Gone;
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
            LitterSelectionLayout.Visibility = ViewStates.Visible;
            NameSelectionLayout.Visibility = ViewStates.Gone;
            PasswordSelectionLayout.Visibility = ViewStates.Gone;
            FinishLayout.Visibility = ViewStates.Gone;

            LitterSwipe.Refreshing = true;
            LitterEmptySwipe.Refreshing = true;
            LitterSwipe.Visibility = ViewStates.Visible;
            LitterEmptySwipe.Visibility = ViewStates.Gone;

            OnLittersLoaded();
        }

        #endregion

        #region Litter

        private void OnLittersLoaded()
        {
            SelectedLitter = null;
            LitterContinue.Enabled = false;

            Departments = new List<Department>(LittersList.FindAll(x => x.room.number == SelectedRoom.House.number).Select(l => new Department(l, false)));
            LitterAdapter = new LittersAdapter(Departments);
            LitterAdapter.ItemClick += Litter_Click;
            LitterRecycler.SetAdapter(LitterAdapter);

            if (Departments.Count == 0)
            {
                LitterEmptySwipe.Visibility = ViewStates.Visible;
                LitterSwipe.Visibility = ViewStates.Gone;
                LitterEmptySwipe.Refreshing = false;
            }
            else
            {
                LitterEmptySwipe.Visibility = ViewStates.Gone;
                LitterSwipe.Visibility = ViewStates.Visible;
                LitterSwipe.Refreshing = false;
            }
        }

        private void Litter_Click(object sender, int pos)
        {
            //Click listener
            if (SelectedLitter == null)
            {
                //Nada seleccionado
                SelectedLitter = Departments[pos];
                Departments[pos].Selected = true;
                LitterAdapter.NotifyItemChanged(pos);
            }
            else
            {
                if (SelectedLitter == Departments[pos])
                {
                    //Click en el que ya está seleccionado (Deseleccionar)
                    SelectedLitter.Selected = false;
                    SelectedLitter = null;
                    LitterAdapter.NotifyItemChanged(pos);
                }
                else
                {
                    //Click en cualquier otro (Selección del nuevo y deselección del antiguo)
                    int prevPos = Departments.FindIndex(x => x == SelectedLitter);
                    SelectedLitter.Selected = false;
                    LitterAdapter.NotifyItemChanged(prevPos);

                    SelectedLitter = Departments[pos];
                    SelectedLitter.Selected = true;
                    LitterAdapter.NotifyItemChanged(pos);
                }
            }
            NotifyLitterSelection();
        }

        private void NotifyLitterSelection()
        {
            if (SelectedLitter != null)
            {
                LitterContinue.Enabled = true;
            }
            else
            {
                LitterContinue.Enabled = false;
            }
        }

        private void Litter_Refresh(object sender, EventArgs e)
        {
            OnLittersLoaded();
        }

        private void LitterContinue_Click(object sender, EventArgs e)
        {
            ConfigModeLayout.Visibility = ViewStates.Gone;
            WifiSelectionLayout.Visibility = ViewStates.Gone;
            RoomSelectionLayout.Visibility = ViewStates.Gone;
            LitterSelectionLayout.Visibility = ViewStates.Gone;
            NameSelectionLayout.Visibility = ViewStates.Visible;
            PasswordSelectionLayout.Visibility = ViewStates.Gone;
            FinishLayout.Visibility = ViewStates.Gone;
        }

        #endregion

        #region Name

        private void NameContinue_Click(object sender, EventArgs e)
        {
            Functions.HideKeyboard(Activity);
            SelectedName = NameInput.Text;
            NameInput.ClearFocus();

            ConfigModeLayout.Visibility = ViewStates.Gone;
            WifiSelectionLayout.Visibility = ViewStates.Gone;
            RoomSelectionLayout.Visibility = ViewStates.Gone;
            LitterSelectionLayout.Visibility = ViewStates.Gone;
            NameSelectionLayout.Visibility = ViewStates.Gone;
            PasswordSelectionLayout.Visibility = ViewStates.Visible;
            FinishLayout.Visibility = ViewStates.Gone;
        }

        #endregion

        #region Password

        private async void PasswordContinue_Click(object sender, EventArgs e)
        {
            Functions.HideKeyboard(Activity);
            PasswordInput.ClearFocus();

            Functions.Loading(true);
            

            //ADD ALL
            string uri = $"http://192.168.4.1/setup?" +
                $"ssid={DefaultNetwork.SSID.Trim('\"')}" +
                $"&passwd={PasswordInput.Text}" +
                $"&mqttserver={Data.MyAddress.ToMqtt()}" +
                $"&mqttport={MqttClient.PORT}" +
                $"&center={SelectedRoom.House.center.idCenter}" +
                $"&typeroom={SelectedRoom.House.type.idRoomType}" +
                $"&number={SelectedRoom.House.number}" +
                $"&department={SelectedLitter.Litter.department}";
            using(var client = new HttpClient())
            {
                try
                {
                    await client.GetAsync(uri);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Log.Error("SCALE", ex.Message);
                    Toast.MakeText(Context, "ERROR", ToastLength.Short).Show();
                }
            }

            await Task.Delay(2000);

            //Desconexión del punto de acceso
            ConnectivityManager connectivityManager = (ConnectivityManager)
                Context.GetSystemService(Context.ConnectivityService);
            connectivityManager.UnregisterNetworkCallback(myWifiCallback);
            connectivityManager.BindProcessToNetwork(null);

            WifiInfo info = myWifiManager.ConnectionInfo;
            DateTime changingWifi = DateTime.Now;
            Ping ping = new Ping();
            PingReply reply;
            bool timeout = false;
            await Task.Run(() =>
            {
                while (info.SSID != DefaultNetwork.SSID)
                {
                    info = myWifiManager.ConnectionInfo;
                    if (DateTime.Now.Subtract(changingWifi).Seconds >= 30)
                    {
                        timeout = true;
                        break;
                    }
                }

                do
                {
                    reply = ping.Send(Data.MyAddress.Server);
                    Log.Info("MQTT", "PING REPLY - " + reply.Status.ToString());
                    if (DateTime.Now.Subtract(changingWifi).Seconds >= 30)
                    {
                        timeout = true;
                        break;
                    }
                }
                while (reply.Status != IPStatus.Success);
            });
            if (timeout)
            {
                Functions.Loading(false);
                Toast.MakeText(Context, Resource.String.wrong_wifi, ToastLength.Long).Show();
                Dismiss();
            }

            var scale = await RequestAPI.GetScaleByMac(SelectedMac);
            EBlackBoxesLocation location = new EBlackBoxesLocation(scale, SelectedRoom.House.center, SelectedRoom.House, SelectedLitter.Litter, DateTime.Now, null);

            MqttClient mqttClient = new MqttClient();
            mqttClient.Client.UseConnectedHandler(async e =>
            {
                Log.Info("MQTT", "CONNECTED");
                try
                {
                    await mqttClient.SubscribeAsync($"scale/{SelectedRoom.House.center.idCenter}/{SelectedRoom.House.type.idRoomType}/{SelectedRoom.House.number}/{SelectedLitter.Litter.department}/{SelectedMac}/configmode");
                    Log.Info("MQTT", "SUBSCRIBED");
                }
                catch (Exception ex)
                {
                    Log.Error("MQTT", ex.Message);
                }
            });
            mqttClient.Client.UseDisconnectedHandler(e =>
            {
                Log.Info("MQTT", "DISCONNECTED");
            });
            mqttClient.Client.UseApplicationMessageReceivedHandler(async e =>
            {
                string[] topic = e.ApplicationMessage.Topic.Split("/");
                string value = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                Log.Info("MQTT", $"MESSAGE RECEIVED - {value}");

                if (value == "on")
                {
                    bool resultLocation = await RequestAPI.InsertScaleLocation(location);
                    scale.name = SelectedName;
                    bool resultName = await RequestAPI.UpdateScale(scale);
                    if (resultLocation && resultName)
                    {
                        try
                        {
                            await mqttClient.PublishAsync($"scale/{SelectedRoom.House.center.idCenter}/{SelectedRoom.House.type.idRoomType}/{SelectedRoom.House.number}/{SelectedLitter.Litter.department}/{SelectedMac}/set-configmode", "off", false);
                            Log.Info("MQTT", "MESSAGE SENT");
                        }
                        catch (Exception ex)
                        {
                            Log.Error("MQTT", ex.Message);
                        }
                    }
                }
                else
                {
                    try
                    {
                        await mqttClient.Client.UnsubscribeAsync();
                        Log.Info("MQTT", "UNSUBSCRIBED");
                        mqttClient.Client.Dispose();
                        Log.Info("MQTT", "CLOSING CONNECTION");

                        Activity.RunOnUiThread(() => MqttFinished());
                    }
                    catch (Exception ex)
                    {
                        Log.Error("MQTT", ex.Message);
                    }
                }
            });
            try
            {
                await mqttClient.ConnectAsync();
                Log.Info("MQTT", "STARTING CONNECTION");
            }
            catch (Exception ex)
            {
                Log.Error("MQTT", ex.Message);
                Functions.Loading(false);
                Toast.MakeText(Context, "ERROR", ToastLength.Short).Show();
                Dismiss();
            }
        }

        private void MqttFinished()
        {
            Functions.Loading(false);

            ConfigModeLayout.Visibility = ViewStates.Gone;
            WifiSelectionLayout.Visibility = ViewStates.Gone;
            RoomSelectionLayout.Visibility = ViewStates.Gone;
            LitterSelectionLayout.Visibility = ViewStates.Gone;
            NameSelectionLayout.Visibility = ViewStates.Gone;
            PasswordSelectionLayout.Visibility = ViewStates.Gone;
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

    class BlinkCallback : ConnectivityManager.NetworkCallback
    {
        ConnectivityManager CM;
        Action<bool> Finished;

        public BlinkCallback(ConnectivityManager cm, Action<bool> finished)
        {
            CM = cm;
            Finished = finished;
        }

        public override async void OnAvailable(Android.Net.Network network)
        {
            base.OnAvailable(network);
            CM.BindProcessToNetwork(network);

            //Blink
            string uri = "http://192.168.4.1/blink";
            using(var client = new HttpClient())
            {
                try
                {
                    await client.GetAsync(uri);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR al lanzar el blink: " + ex.Message);
                    Log.Error("BLINK", ex.Message);
                }
            }
            
            //Fin
            CM.UnregisterNetworkCallback(this);
            CM.BindProcessToNetwork(null);

            Finished(true);
        }

        public override void OnUnavailable()
        {
            base.OnUnavailable();

            CM.UnregisterNetworkCallback(this);
            Finished(false);
        }
    }

    class WifiCallback : ConnectivityManager.NetworkCallback
    {
        ConnectivityManager CM;
        Action<bool, ConnectivityManager> Finished;

        public WifiCallback(ConnectivityManager cm, Action<bool, ConnectivityManager> finished)
        {
            CM = cm;
            Finished = finished;
        }

        public override void OnAvailable(Android.Net.Network network)
        {
            base.OnAvailable(network);
            CM.BindProcessToNetwork(network);
            Finished(true, CM);
        }

        public override void OnUnavailable()
        {
            base.OnUnavailable();
            CM.UnregisterNetworkCallback(this);
            Finished(false, CM);
        }
    }
}