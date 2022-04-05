
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AVG_access_data;
using MQTTnet.Extensions.ManagedClient;
using Encoding = System.Text.Encoding;

namespace AVG_Scale_Installer
{
    public class Scale : Fragment
    {
        private TextView WeightValue;
        private Button IdentifyButton;
        private Button TareButton;
        private Button CalibrateButton;
        MqttClient mqttClient;
        EBlackBoxesScale SelectedScale;
        private ERoom SelectedRoom;
        private ELitter SelectedLitter;
        int TareValue;
        int LastValue;
        DateTime LastReading;
        bool Alive;

        public Scale(EBlackBoxesScale scale, ERoom room, ELitter litter)
        {
            SelectedScale = scale;
            SelectedRoom = room;
            SelectedLitter = litter;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.Scale, container, false);

            ((MainActivity)Activity).SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            ((MainActivity)Activity).SupportActionBar.SetDisplayShowHomeEnabled(true);

            WeightValue = view.FindViewById<TextView>(Resource.Id.ScaleValueTextView);
            IdentifyButton = view.FindViewById<Button>(Resource.Id.ScaleIdentifyButton);
            TareButton = view.FindViewById<Button>(Resource.Id.ScaleTareButton);
            CalibrateButton = view.FindViewById<Button>(Resource.Id.ScaleCalibrateButton);

            IdentifyButton.Click += IdentifyButton_Click;
            TareButton.Click += TareButton_Click;
            CalibrateButton.Click += CalibrateButton_Click;

            return view;
        }

        private async void IdentifyButton_Click(object sender, EventArgs e)
        {
            if (mqttClient != null && mqttClient.Client != null && mqttClient.Client.IsConnected)
            {
                try
                {
                    await mqttClient.PublishAsync($"scale/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/{SelectedLitter.department}/{SelectedScale.mac}/set-blinking", "on", false);
                    Log.Info("MQTT", "BLINKING SENT");
                }
                catch (Exception ex)
                {
                    Log.Error("MQTT", ex.Message);
                }
            }
        }

        private void TareButton_Click(object sender, EventArgs e)
        {
            TareValue = LastValue;
            WeightValue.Text = (LastValue - TareValue).ToString();
        }

        private async void CalibrateButton_Click(object sender, EventArgs e)
        {
            Alive = false;
            if (mqttClient != null)
            {
                try
                {
                    await mqttClient.PublishAsync($"scale/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/{SelectedLitter.department}/{SelectedScale.mac}/set-continuousweight", "off", false);
                    Log.Info("MQTT", "MESSAGE SENT");
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

            ScaleCalibration dialog = new ScaleCalibration(SelectedScale, SelectedRoom, SelectedLitter, OnResume);
            dialog.Show(Activity.SupportFragmentManager, null);
        }

        public async override void OnResume()
        {
            base.OnResume();
            TareValue = -10000;
            LastValue = 0;
            LastReading = DateTime.Now;

            mqttClient = new MqttClient();
            mqttClient.Client.UseConnectedHandler(async e =>
            {
                Log.Info("MQTT", "CONNECTED");
                try
                {
                    await mqttClient.SubscribeAsync($"scale/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/{SelectedLitter.department}/{SelectedScale.mac}/continuousweight");
                    Log.Info("MQTT", "SUBSCRIBED");

                    //TEMPORAL
                    //await mqttClient.SubscribeAsync($"scale/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/{SelectedLitter.department}/{SelectedScale.mac}/weight");
                    //Log.Info("MQTT", "SUBSCRIBED TEMPORAL");
                    //FIN TEMPORAL

                    await mqttClient.PublishAsync($"scale/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/{SelectedLitter.department}/{SelectedScale.mac}/set-continuousweight", "on", false);
                    Log.Info("MQTT", "MESSAGE SENT");
                }
                catch (Exception ex)
                {
                    Log.Error("MQTT", ex.Message);
                }
                Activity.RunOnUiThread(() =>
                {
                    IdentifyButton.Enabled = true;
                    CalibrateButton.Enabled = true;
                });
            });
            mqttClient.Client.UseDisconnectedHandler(e =>
            {
                Log.Info("MQTT", "DISCONNECTED");
                Activity.RunOnUiThread(() =>
                {
                    IdentifyButton.Enabled = false;
                    TareButton.Enabled = false;
                    CalibrateButton.Enabled = false;
                });
            });
            mqttClient.Client.UseApplicationMessageReceivedHandler(async e =>
            {
                
                string[] topic = e.ApplicationMessage.Topic.Split("/");

                if (topic.Last() == "weight")
                {
                    Activity.RunOnUiThread(() =>
                    {
                        //CalibrateButton.Text = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    });
                }
                else
                {
                    Log.Info("MQTT", "MESSAGE RECEIVED WEIGHT");
                    string value = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    int.TryParse(value,out LastValue);
                    LastReading = DateTime.Now;

                    if (TareValue == -10000)
                    {
                        TareValue = LastValue;
                    }

                    Activity.RunOnUiThread(() =>
                    {
                        WeightValue.Text = (LastValue - TareValue).ToString();
                        TareButton.Enabled = true;
                    });
                    Log.Info("PESO", $"CRUDO: {value} | TARA: {TareValue} | PESO: {(LastValue - TareValue)}");
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

            Task.Run(() =>
            {
                Alive = true;
                while (Alive)
                {
                    if(DateTime.Now.Subtract(LastReading).Seconds > 30)
                    {
                        if (mqttClient.Client.IsConnected)
                        {
                            try
                            {
                                LastReading = DateTime.Now;
                                mqttClient.PublishAsync($"scale/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/{SelectedLitter.department}/{SelectedScale.mac}/set-continuousweight", "on", false);
                                Log.Info("MQTT", "MESSAGE SENT ALIVE");
                            }
                            catch (Exception ex)
                            {
                                Log.Error("MQTT", ex.Message);
                            }
                        }
                        else
                        {
                            Activity.OnBackPressed();
                        }
                    }
                }
            });
        }

        public async override void OnPause()
        {
            base.OnPause();
            Alive = false;
            if(mqttClient != null)
            {
                try
                {
                    await mqttClient.PublishAsync($"scale/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/{SelectedLitter.department}/{SelectedScale.mac}/set-continuousweight", "off", false);
                    Log.Info("MQTT", "MESSAGE SENT STOP ALIVE");
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
    }
}
