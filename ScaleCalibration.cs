
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Fragment.App;
using AVG_access_data;
using Felipecsl.GifImageViewLibrary;
using MQTTnet.Extensions.ManagedClient;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace AVG_Scale_Installer
{
    public class ScaleCalibration : DialogFragment
    {
        private LinearLayout InfoLayout;
        private WebView InfoText;
        private Button InfoContinue;
        private LinearLayout WaitLayout;
        private WebView WaitText;
        private GifImageView WaitGif;
        private LinearLayout WeightLayout;
        private EditText WeightInput;
        private Button WeightContinue;
        private LinearLayout FinishLayout;
        private Button FinishButton;
        private MqttClient mqttClient;
        private EBlackBoxesScale SelectedScale;
        private ERoom SelectedRoom;
        private ELitter SelectedLitter;
        private Action ParentResume;

        public ScaleCalibration(EBlackBoxesScale scale, ERoom room, ELitter litter, Action onResume)
        {
            SelectedScale = scale;
            SelectedRoom = room;
            SelectedLitter = litter;
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
            View view = inflater.Inflate(Resource.Layout.ScaleCalibration, container, false);

            //Toolbar
            Toolbar toolbar = view.FindViewById<Toolbar>(Resource.Id.ScaleCalibrationToolbar);
            //toolbar.SetTitle();
            toolbar.SetNavigationIcon(Resource.Drawable.clear);
            toolbar.NavigationClick += delegate { Dismiss(); };

            //Pantalla Info
            InfoLayout = view.FindViewById<LinearLayout>(Resource.Id.ScaleCalibrationInfoLayout);
            InfoText = view.FindViewById<WebView>(Resource.Id.ScaleCalibrationInfoText);
            InfoContinue = view.FindViewById<Button>(Resource.Id.ScaleCalibrationInfoContinue);
            string info = "<html><body text='white' style='text-align:justify;'>" + Resources.GetText(Resource.String.scale_calibration_info) + "</body></html>";
            InfoText.SetBackgroundColor(Color.Transparent);
            InfoText.LoadData(info, "text/html; charset=utf-8", "UTF-8");
            InfoContinue.Click += InfoContinue_Click;

            //Pantalla Esperar
            WaitLayout = view.FindViewById<LinearLayout>(Resource.Id.ScaleCalibrationWaitLayout);
            WaitText = view.FindViewById<WebView>(Resource.Id.ScaleCalibrationWaitText);
            WaitGif = view.FindViewById<GifImageView>(Resource.Id.ScaleCalibrationWaitGif);
            Stream input = Resources.OpenRawResource(Resource.Drawable.chick1);
            byte[] bytes = DoingWork.ConvertByteArray(input);
            WaitGif.SetBytes(bytes);
            WaitGif.StartAnimation();

            string wait = "<html><body text='white' style='text-align:justify;'>" + Resources.GetText(Resource.String.scale_calibration_wait) + "</body></html>";
            WaitText.SetBackgroundColor(Color.Transparent);
            WaitText.LoadData(wait, "text/html; charset=utf-8", "UTF-8");

            //Pantalla Peso
            WeightLayout = view.FindViewById<LinearLayout>(Resource.Id.ScaleCalibrationWeightLayout);
            WeightInput = view.FindViewById<EditText>(Resource.Id.ScaleCalibrationWeightInput);
            WeightContinue = view.FindViewById<Button>(Resource.Id.ScaleCalibrationWeightContinue);
            WeightInput.AfterTextChanged += CheckVoid;
            WeightContinue.Click += WeightContinue_Click;

            //Pantalla Fin
            FinishLayout = view.FindViewById<LinearLayout>(Resource.Id.ScaleCalibrationFinishLayout);
            FinishButton = view.FindViewById<Button>(Resource.Id.ScaleCalibrationFinishButton);
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
                    await mqttClient.SubscribeAsync($"scale/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/{SelectedLitter.department}/{SelectedScale.mac}/calibration/zeroweight");
                    Log.Info("MQTT", "SUBSCRIBED TO ZERO WEIGHT");
                    await mqttClient.SubscribeAsync($"scale/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/{SelectedLitter.department}/{SelectedScale.mac}/calibration/divisor");
                    Log.Info("MQTT", "SUBSCRIBED TO DIVISOR");
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
                    //FINALIZADO LA FASE VACIO
                    case "zeroweight":
                        Log.Info("MQTT", "MESSAGE RECEIVED ZERO");
                        SelectedScale.valorCero = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                        Activity.RunOnUiThread(() =>
                        {
                            InfoLayout.Visibility = ViewStates.Gone;
                            WaitLayout.Visibility = ViewStates.Gone;
                            WeightLayout.Visibility = ViewStates.Visible;
                            FinishLayout.Visibility = ViewStates.Gone;
                        });

                        break;

                    //FINALIZADO LA FASE PESO CONOCIDO
                    case "divisor":
                        Log.Info("MQTT", "MESSAGE RECEIVED DIVISOR");
                        SelectedScale.divisor = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                        bool result = await RequestAPI.UpdateScale(SelectedScale);

                        Activity.RunOnUiThread(() =>
                        {
                            if (result)
                            {
                                InfoLayout.Visibility = ViewStates.Gone;
                                WaitLayout.Visibility = ViewStates.Gone;
                                WeightLayout.Visibility = ViewStates.Gone;
                                FinishLayout.Visibility = ViewStates.Visible;
                            }
                            else
                            {
                                Toast.MakeText(Context, "ERROR", ToastLength.Short).Show();
                                Dismiss();
                            }
                            
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

        private void CheckVoid(object sender, AfterTextChangedEventArgs e)
        {
            EditText input = (EditText)sender;
            if (input.Text.Length == 0)
            {
                WeightContinue.Enabled = false;
            }
            else
            {
                WeightContinue.Enabled = true;
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
                    await mqttClient.PublishAsync($"scale/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/{SelectedLitter.department}/{SelectedScale.mac}/set-calibration", "0", false);
                    Log.Info("MQTT", "MESSAGE SENT ZERO");

                    InfoLayout.Visibility = ViewStates.Gone;
                    WaitLayout.Visibility = ViewStates.Visible;
                    WeightLayout.Visibility = ViewStates.Gone;
                    FinishLayout.Visibility = ViewStates.Gone;
                }
                catch (Exception ex)
                {
                    Log.Error("MQTT", ex.Message);
                }
            }
        }

        #endregion

        #region Wait



        #endregion

        #region Weight

        private async void WeightContinue_Click(object sender, EventArgs e)
        {
            if (mqttClient != null && mqttClient.Client.IsConnected)
            {
                try
                {
                    await mqttClient.PublishAsync($"scale/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/{SelectedLitter.department}/{SelectedScale.mac}/set-calibration", WeightInput.Text, false);
                    Log.Info("MQTT", "MESSAGE SENT WEIGHT");

                    InfoLayout.Visibility = ViewStates.Gone;
                    WaitLayout.Visibility = ViewStates.Visible;
                    WeightLayout.Visibility = ViewStates.Gone;
                    FinishLayout.Visibility = ViewStates.Gone;
                }
                catch (Exception ex)
                {
                    Log.Error("MQTT", ex.Message);
                }
            }
        }

        #endregion

        #region Finish

        public override void Dismiss()
        {
            base.Dismiss();
            ParentResume.Invoke();
        }

        #endregion

    }
}
