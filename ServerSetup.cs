using Android;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AndroidX.Fragment.App;
using AVG_Scale_Installer.Tools;
using Firebase.CodeScanner;
using Google.ZXing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace AVG_Scale_Installer
{
    public class ServerSetup : Fragment, IDecodeCallback
    {
        private LinearLayout Parent;
        private LinearLayout Logos;
        private WebView InfoText;
        private EditText ServerInput;
        private Button ScanButton;
        private TextView ErrorText;
        private CodeScannerView ScannerView;
        private CodeScanner _CodeScanner;
        private Button ContinueButton;
        private ISharedPreferences Prefs;
        private const int CAMERA_REQUEST_CODE = 101;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Data.CurrentFM = Activity.SupportFragmentManager;

            Prefs = PreferenceManager.GetDefaultSharedPreferences(Context);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.ServerSetup, container, false);

            Parent = view.FindViewById<LinearLayout>(Resource.Id.ServerSetupParentLayout);
            Logos = view.FindViewById<LinearLayout>(Resource.Id.ServerSetupLogosLayout);

            InfoText = view.FindViewById<WebView>(Resource.Id.ServerSetupInfoText);
            string info = "<html><body text='white' style='text-align:justify;'>" + Resources.GetText(Resource.String.server_info) + "</body></html>";
            InfoText.SetBackgroundColor(Color.Transparent);
            InfoText.LoadData(info, "text/html; charset=utf-8", "UTF-8");

            ServerInput = view.FindViewById<EditText>(Resource.Id.ServerSetupServerInput);
            ScanButton = view.FindViewById<Button>(Resource.Id.ServerSetupScanButton);

            ErrorText = view.FindViewById<TextView>(Resource.Id.ServerSetupErrorText);

            ScannerView = view.FindViewById<CodeScannerView>(Resource.Id.ServerSetupScannerView);
            _CodeScanner = new CodeScanner(Context, ScannerView)
            {
                ScanMode = ScanMode.Continuous,
                AutoFocusEnabled = true,
                AutoFocusMode = AutoFocusMode.Continuous
            };

            ContinueButton = view.FindViewById<Button>(Resource.Id.ServerSetupContinueButton);

            view.FindViewById<LinearLayout>(Resource.Id.ServerSetupLayout).Touch += delegate
            {
                Functions.HideKeyboard(Activity);
                ServerInput.ClearFocus();
            };

            ServerInput.AfterTextChanged += ServerInput_AfterTextChanged;

            ScanButton.Click += ScanButton_Click;
            ContinueButton.Click += ContinueButton_Click;

            return view;
        }

        public async override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            if(Prefs.GetString("server", "") != "")
            {
                Logos.Visibility = ViewStates.Visible;
                Activity.StartActivity(new Intent(Activity, typeof(MainActivity)));
            }
            else
            {
                Parent.Visibility = ViewStates.Visible;
            }
        }

        private void ServerInput_AfterTextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            ContinueButton.Enabled = ServerInput.Text.Length > 0;
        }

        private void ScanButton_Click(object sender, EventArgs e)
        {
            if (ContextCompat.CheckSelfPermission(Context, Manifest.Permission.Camera) == Android.Content.PM.Permission.Granted)
            {
                StartPreview();
                return;
            }

            ActivityCompat.RequestPermissions(Activity, new[] { Manifest.Permission.Camera }, 1001);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode == 1001)
            {
                if (permissions.Length > 0 && grantResults.Length > 0 && permissions[0].Equals(Manifest.Permission.Camera) &&
                    grantResults[0] == Android.Content.PM.Permission.Granted)
                {
                    StartPreview();
                }
            }
        }

        private void StartPreview()
        {
            ScannerView.Visibility = ViewStates.Visible;
            _CodeScanner.DecodeCallback = this;
            _CodeScanner.StartPreview();
        }

        public override void OnPause()
        {
            base.OnPause();
            _CodeScanner.StopPreview();
            _CodeScanner.DecodeCallback = null;
            ScannerView.Visibility = ViewStates.Invisible;
        }

        private async void ContinueButton_Click(object sender, EventArgs e)
        {
            ContinueButton.Enabled = false;
            await Task.Run(() => Functions.Loading(true));
            ErrorText.Visibility = ViewStates.Gone;
            try
            {
                Ping ping = new Ping();
                var ip = ServerInput.Text.Substring(0, ServerInput.Text.IndexOf(":"));
                var reply = ping.Send(ip);

                if(reply.Status == IPStatus.Success)
                {
                    Prefs.Edit().PutString("server", ServerInput.Text).Apply();
                    Parent.Visibility = ViewStates.Gone;
                    await Task.Run(() => Functions.Loading(false));
                    Logos.Visibility = ViewStates.Visible;
                    Activity.StartActivity(new Intent(Activity, typeof(MainActivity)));
                    
                }
                else
                {
                    await Task.Delay(2000);
                    ErrorText.Visibility = ViewStates.Visible;
                    await Task.Run(() => Functions.Loading(false));
                    ContinueButton.Enabled = true;
                }
            }
            catch(Exception ex)
            {
                ErrorText.Visibility = ViewStates.Visible;
                await Task.Run(() => Functions.Loading(false));
                ContinueButton.Enabled = true;
            }

        }

        public void OnDecoded(Result p0)
        {
            _CodeScanner.StopPreview();
            _CodeScanner.DecodeCallback = null;
            Activity.RunOnUiThread(() =>
            {
                ServerInput.Text = p0.Text;
                ScannerView.Visibility = ViewStates.Invisible;
            });
        }
    }
}