
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.Fragment.App;
using AVG_access_data;
using Felipecsl.GifImageViewLibrary;
using MQTTnet.Extensions.ManagedClient;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace AVG_Scale_Installer
{
    public class BlackBoxWindCalibration : DialogFragment
    {
        private EBlackBoxesBlackBox SelectedBlackBox;
        private ERoom SelectedRoom;
        private Action ParentResume;
        private LinearLayout InfoLayout;
        private WebView InfoText;
        private Button InfoContinue;
        private LinearLayout WaitLayout;
        private WebView WaitText;
        private GifImageView WaitGif;
        private LinearLayout FinishLayout;
        private Button FinishButton;
        private MqttClient mqttClient;

        public BlackBoxWindCalibration(EBlackBoxesBlackBox blackbox, ERoom room, Action onResume)
        {
            SelectedBlackBox = blackbox;
            SelectedRoom = room;
            ParentResume = onResume;
        }

        public override void OnStart()
        {
            base.OnStart();

            Android.App.Dialog dialog = Dialog;
            if (dialog != null)
            {
                int width = ViewGroup.LayoutParams.MatchParent;
                int height = ViewGroup.LayoutParams.MatchParent;
                dialog.Window.SetLayout(width, height);
            }
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(DialogFragment.StyleNormal, Resource.Style.AppTheme_FullScreenDialog);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.BlackBoxWindCalibration, container, false);

            //Toolbar
            Toolbar toolbar = view.FindViewById<Toolbar>(Resource.Id.WindCalibrationToolbar);
            //toolbar.SetTitle();
            toolbar.SetNavigationIcon(Resource.Drawable.clear);
            toolbar.NavigationClick += delegate { Dismiss(); };

            //Pantalla Info
            InfoLayout = view.FindViewById<LinearLayout>(Resource.Id.WindCalibrationInfoLayout);
            InfoText = view.FindViewById<WebView>(Resource.Id.WindCalibrationInfoText);
            InfoContinue = view.FindViewById<Button>(Resource.Id.WindCalibrationInfoContinue);
            string info = "<html><body text='white' style='text-align:justify;'>" + Resources.GetText(Resource.String.wind_calibration_info) + "</body></html>";
            InfoText.SetBackgroundColor(Color.Transparent);
            InfoText.LoadData(info, "text/html; charset=utf-8", "UTF-8");
            InfoContinue.Click += InfoContinue_Click;

            //Pantalla Esperar
            WaitLayout = view.FindViewById<LinearLayout>(Resource.Id.WindCalibrationWaitLayout);
            WaitText = view.FindViewById<WebView>(Resource.Id.WindCalibrationWaitText);
            WaitGif = view.FindViewById<GifImageView>(Resource.Id.WindCalibrationWaitGif);
            Stream input = Resources.OpenRawResource(Resource.Drawable.chick1);
            byte[] bytes = DoingWork.ConvertByteArray(input);
            WaitGif.SetBytes(bytes);
            WaitGif.StartAnimation();

            string wait = "<html><body text='white' style='text-align:justify;'>" + Resources.GetText(Resource.String.wind_calibration_wait) + "</body></html>";
            WaitText.SetBackgroundColor(Color.Transparent);
            WaitText.LoadData(wait, "text/html; charset=utf-8", "UTF-8");

            //Pantalla Fin
            FinishLayout = view.FindViewById<LinearLayout>(Resource.Id.WindCalibrationFinishLayout);
            FinishButton = view.FindViewById<Button>(Resource.Id.WindCalibrationFinishButton);
            FinishButton.Click += delegate
            {
                Dismiss();
            };

            return view;
        }

        #region Workflow

        public async override void OnResume()
        {
            base.OnResume();

            mqttClient = new MqttClient();
            mqttClient.Client.UseConnectedHandler(async e =>
            {
                Log.Info("MQTT", "CONNECTED");
                try
                {
                    await mqttClient.SubscribeAsync($"blackbox/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/0/{SelectedBlackBox.mac}/calibration/windzero");
                    Log.Info("MQTT", "SUBSCRIBED TO WIND ZERO");
                }
                catch (Exception ex)
                {
                    Log.Error("MQTT", ex.Message);
                }
                Activity.RunOnUiThread(() =>
                {
                    InfoContinue.Enabled = true;
                });
            });
            mqttClient.Client.UseDisconnectedHandler(e =>
            {
                Log.Info("MQTT", "DISCONNECTED");
                Activity.RunOnUiThread(() =>
                {
                    InfoContinue.Enabled = false;
                });
            });
            mqttClient.Client.UseApplicationMessageReceivedHandler(async e =>
            {
                string[] topic = e.ApplicationMessage.Topic.Split("/");

                switch (topic.Last())
                {
                    //FINALIZADO LA FASE VIENTO CERO
                    case "windzero":
                        Log.Info("MQTT", "MESSAGE RECEIVED WIND ZERO");
                        Activity.RunOnUiThread(() =>
                        {
                            InfoLayout.Visibility = ViewStates.Gone;
                            WaitLayout.Visibility = ViewStates.Gone;
                            FinishLayout.Visibility = ViewStates.Visible;
                        });

                        break;
                    default:
                        break;
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
            }
        }

        public async override void OnPause()
        {
            base.OnPause();
            if (mqttClient != null)
            {
                try
                {
                    await mqttClient.Client.UnsubscribeAsync();
                    Log.Info("MQTT", "UNSUBSCRIBED");
                    mqttClient.Client.Dispose();
                    Log.Info("MQTT", "CLOSING CONNECTION");
                }
                catch (Exception ex)
                {
                    Log.Error("MQTT", ex.Message);
                }
            }
        }

        #endregion

        #region Info

        private async void InfoContinue_Click(object sender, EventArgs e)
        {
            if (mqttClient != null && mqttClient.Client.IsConnected)
            {
                try
                {
                    await mqttClient.PublishAsync($"blackbox/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/0/{SelectedBlackBox.mac}/set-windzero", "on", false);
                    Log.Info("MQTT", "MESSAGE SENT WIND ZERO");

                    InfoLayout.Visibility = ViewStates.Gone;
                    WaitLayout.Visibility = ViewStates.Visible;
                    FinishLayout.Visibility = ViewStates.Gone;
                }
                catch (Exception ex)
                {
                    Log.Error("MQTT", ex.Message);
                }
            }
        }

        #endregion

        public override void Dismiss()
        {
            base.Dismiss();
            ParentResume.Invoke();
        }
    }
}
