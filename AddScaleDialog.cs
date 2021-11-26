
using Android;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Net.Wifi;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using AVG_Scale_Installer.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbar = Android.Support.V7.Widget.Toolbar;

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
        private WifiMonitor ReceiverWifi;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
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
            string info = "<html><body text='white' style='text-align:justify;'>" + Resources.GetText(Resource.String.config_mode_info) + "</body></html>";
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
            WifiSwipe.Refresh += Swipe_Refresh;
            WifiEmptySwipe.Refresh += Swipe_Refresh;
            WifiIdentify.Click += WifiIdentify_Click;
            WifiContinue.Click += WifiContinue_Click;

            //Pantalla seleccionar nave


            //Workflow
            view.FindViewById<LinearLayout>(Resource.Id.AddScaleDialogLayout).Touch += delegate
            {
                Functions.HideKeyboard(Activity);
            };

            return view;
        }


        #region Workflow

        public override void OnResume()
        {
            base.OnResume();

            ReceiverWifi = new WifiMonitor(myWifiManager, WifiRecycler, WifiSwipe, WifiEmptySwipe, NotifyNetworkSelection);
            IntentFilter intentFilter = new IntentFilter();
            intentFilter.AddAction(WifiManager.ScanResultsAvailableAction);
            Context.RegisterReceiver(ReceiverWifi, intentFilter);
        }

        public override void OnPause()
        {
            base.OnPause();
            Context.UnregisterReceiver(ReceiverWifi);
            Dismiss();
        }
        
        private void Toolbar_NavigationClick(object sender, Toolbar.NavigationClickEventArgs e)
        {
            Dismiss();
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M && Context.CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) != (int)Permission.Granted)
            {
                RequestPermissions(new string[] { Manifest.Permission.AccessCoarseLocation }, 1);
            }
        }

        #endregion

        #region Config

        private void ConfigModeContinue_Click(object sender, EventArgs e)
        {
            myWifiManager.StartScan();
            WifiSwipe.Refreshing = true;
            WifiEmptySwipe.Refreshing = true;
            WifiSwipe.Visibility = ViewStates.Visible;
            WifiEmptySwipe.Visibility = ViewStates.Gone;

            ConfigModeLayout.Visibility = ViewStates.Gone;
            WifiSelectionLayout.Visibility = ViewStates.Visible;
        }

        #endregion

        #region Wifi

        public void NotifyNetworkSelection()
        {
            if(Data.SelectedNetwork != null)
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

        private void Swipe_Refresh(object sender, EventArgs e)
        {
            myWifiManager.StartScan();
        }

        private async void WifiIdentify_Click(object sender, EventArgs e)
        {
            Functions.ForceWifiOverCellular(Context);
            WifiIdentify.Enabled = false;
            WifiContinue.Enabled = false;
            var connected = false;
            await Task.Run(async () =>
            {
                Functions.Loading(true);

                //Wifi del centro (Original)
                var centerWifi = myWifiManager.ConnectionInfo;


            });
        }

        private void WifiContinue_Click(object sender, EventArgs e)
        {

        }

        

        #endregion

    }
}