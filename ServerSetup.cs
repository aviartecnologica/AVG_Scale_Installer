using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.Fragment.App;
using AVG_Scale_Installer.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Google.MLKit.Vision.BarCode;

namespace AVG_Scale_Installer
{
    public class ServerSetup : Fragment
    {
        private LinearLayout Parent;
        private LinearLayout Logos;
        private WebView InfoText;
        private EditText ServerInput;
        private Button ScanButton;
        private TextView ErrorText;
        private ImageView QrImage;
        private Button ContinueButton;
        private ISharedPreferences Prefs;

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

            QrImage = view.FindViewById<ImageView>(Resource.Id.ServerSetupQrImage);

            ContinueButton = view.FindViewById<Button>(Resource.Id.ServerSetupContinueButton);

            view.FindViewById<LinearLayout>(Resource.Id.ServerSetupLayout).Touch += delegate
            {
                Functions.HideKeyboard(Activity);
                ServerInput.ClearFocus();
            };

            ServerInput.FocusChange += ServerInput_FocusChange;
            ServerInput.AfterTextChanged += ServerInput_AfterTextChanged;

            ScanButton.Click += ScanButton_Click;
            ContinueButton.Click += ContinueButton_Click;

            return view;
        }

        private void ServerInput_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (ServerInput.IsFocused)
            {
                QrImage.Visibility = ViewStates.Gone;
            }
            else
            {
                QrImage.Visibility = ViewStates.Visible;
            }
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
            Toast.MakeText(Context, "Work in progress", ToastLength.Short).Show();

            BarcodeScannerOptions options = new BarcodeScannerOptions.Builder()
                .SetBarcodeFormats(Barcode.FormatQrCode).Build();
        }

        private async void ContinueButton_Click(object sender, EventArgs e)
        {
            ContinueButton.Enabled = false;
            await Task.Run(() => Functions.Loading(true));
            ErrorText.Visibility = ViewStates.Gone;
            try
            {
                Ping ping = new Ping();
                var reply = ping.Send(ServerInput.Text);

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

    }
}