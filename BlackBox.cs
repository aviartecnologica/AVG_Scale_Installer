
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace AVG_Scale_Installer
{
    public class BlackBox : Fragment
    {
        private EBlackBoxesBlackBox SelectedBlackBox;
        private ERoom SelectedRoom;
        private TextView Temperature;
        private TextView Humidity;
        private TextView Ammonia;
        private TextView Wind;
        private TextView Lumen;
        private TextView Sound;
        private TextView Co2;
        private Button IdentifyButton;
        private Button CalibrateWindButton;
        private Button CalibrateCO2Button;
        private MqttClient mqttClient;
        bool Alive;
        private DateTime LastReading;

        public BlackBox(EBlackBoxesBlackBox blackbox, ERoom room)
        {
            SelectedBlackBox = blackbox;
            SelectedRoom = room;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.BlackBox, container, false);

            ((MainActivity)Activity).SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            ((MainActivity)Activity).SupportActionBar.SetDisplayShowHomeEnabled(true);

            Temperature = view.FindViewById<TextView>(Resource.Id.BlackBoxTempText);
            Humidity = view.FindViewById<TextView>(Resource.Id.BlackBoxHumText);
            Ammonia = view.FindViewById<TextView>(Resource.Id.BlackBoxAmmText);
            Wind = view.FindViewById<TextView>(Resource.Id.BlackBoxWindText);
            Lumen = view.FindViewById<TextView>(Resource.Id.BlackBoxLumText);
            Sound = view.FindViewById<TextView>(Resource.Id.BlackBoxSounText);
            Co2 = view.FindViewById<TextView>(Resource.Id.BlackBoxCo2Text);

            IdentifyButton = view.FindViewById<Button>(Resource.Id.BlackBoxIdentifyButton);
            CalibrateWindButton = view.FindViewById<Button>(Resource.Id.BlackBoxCalibrateWindButton);
            CalibrateCO2Button = view.FindViewById<Button>(Resource.Id.BlackBoxCalibrateCO2Button);

            IdentifyButton.Click += IdentifyButton_Click;
            CalibrateWindButton.Click += CalibrateWindButton_Click;
            CalibrateCO2Button.Click += CalibrateCO2Button_Click;

            return view;
        }

        public async override void OnResume()
        {
            base.OnResume();

            LastReading = DateTime.Now;

            CalibrateWindButton.Enabled = true;
            CalibrateCO2Button.Enabled = true;
            
            mqttClient = new MqttClient();
            mqttClient.Client.UseConnectedHandler(async e =>
            {
                Log.Info("MQTT", "CONNECTED");
                try
                {
                    await mqttClient.SubscribeAsync($"blackbox/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/0/{SelectedBlackBox.mac}/temperature");
                    Log.Info("MQTT", "SUBSCRIBED TEMPERATURE");
                    await mqttClient.SubscribeAsync($"blackbox/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/0/{SelectedBlackBox.mac}/humidity");
                    Log.Info("MQTT", "SUBSCRIBED HUMIDITY");
                    await mqttClient.SubscribeAsync($"blackbox/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/0/{SelectedBlackBox.mac}/ammonia");
                    Log.Info("MQTT", "SUBSCRIBED AMMONIA");
                    await mqttClient.SubscribeAsync($"blackbox/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/0/{SelectedBlackBox.mac}/wind");
                    Log.Info("MQTT", "SUBSCRIBED WIND");
                    await mqttClient.SubscribeAsync($"blackbox/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/0/{SelectedBlackBox.mac}/lumen");
                    Log.Info("MQTT", "SUBSCRIBED LUMEN");
                    await mqttClient.SubscribeAsync($"blackbox/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/0/{SelectedBlackBox.mac}/sound");
                    Log.Info("MQTT", "SUBSCRIBED SOUND");
                    await mqttClient.SubscribeAsync($"blackbox/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/0/{SelectedBlackBox.mac}/co2");
                    Log.Info("MQTT", "SUBSCRIBED CO2");

                    await mqttClient.PublishAsync($"blackbox/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/0/{SelectedBlackBox.mac}/set-continuousvalues", "on", false);
                    Log.Info("MQTT", "MESSAGE SENT");
                }
                catch (Exception ex)
                {
                    Log.Error("MQTT", ex.Message);
                }
                Activity.RunOnUiThread(() =>
                {
                    IdentifyButton.Enabled = true;
                });
            });
            mqttClient.Client.UseDisconnectedHandler(e =>
            {
                Log.Info("MQTT", "DISCONNECTED");
                Activity.RunOnUiThread(() =>
                {
                    IdentifyButton.Enabled = false;
                });
            });
            mqttClient.Client.UseApplicationMessageReceivedHandler(async e =>
            {
                LastReading = DateTime.Now;
                string topic = e.ApplicationMessage.Topic.Split("/").Last();
                string value = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                switch (topic)
                {
                    case "temperature":
                        Log.Info("MQTT", $"MESSAGE RECEIVED TEMPERATURE - {value}");
                        double temp = 0;
                        double.TryParse(value, out temp);
                        temp = Math.Round(temp, 1);
                        Activity.RunOnUiThread(() =>
                        {
                            Temperature.Text = temp.ToString();
                        });
                        break;
                    case "humidity":
                        Log.Info("MQTT", $"MESSAGE RECEIVED HUMIDITY - {value}");
                        double hum = 0;
                        double.TryParse(value, out hum);
                        hum = Math.Round(hum, 1);
                        Activity.RunOnUiThread(() =>
                        {
                            Humidity.Text = hum.ToString();
                        });
                        break;
                    case "ammonia":
                        Log.Info("MQTT", $"MESSAGE RECEIVED AMMONIA - {value}");
                        double amm = 0;
                        double.TryParse(value, out amm);
                        var ammInt = (int)Math.Round(amm);
                        Activity.RunOnUiThread(() =>
                        {
                            Ammonia.Text = ammInt.ToString();
                        });
                        break;
                    case "wind":
                        Log.Info("MQTT", $"MESSAGE RECEIVED WIND - {value}");
                        double wind = 0;
                        double.TryParse(value, out wind);
                        wind = Math.Round(wind, 1);
                        Activity.RunOnUiThread(() =>
                        {
                            Wind.Text = wind.ToString();
                        });
                        break;
                    case "lumen":
                        Log.Info("MQTT", $"MESSAGE RECEIVED LUMEN - {value}");
                        double lum = 0;
                        double.TryParse(value, out lum);
                        lum = Math.Round(lum, 1);
                        Activity.RunOnUiThread(() =>
                        {
                            Lumen.Text = lum.ToString();
                        });
                        break;
                    case "sound":
                        Log.Info("MQTT", $"MESSAGE RECEIVED SOUND - {value}");
                        double sou = 0;
                        double.TryParse(value, out sou);
                        var souInt = (int)Math.Round(sou);
                        Activity.RunOnUiThread(() =>
                        {
                            Sound.Text = souInt.ToString();
                        });
                        break;
                    case "co2":
                        Log.Info("MQTT", $"MESSAGE RECEIVED CO2 - {value}");
                        double co2 = 0;
                        double.TryParse(value, out co2);
                        var co2Int = (int)Math.Round(co2);
                        Activity.RunOnUiThread(() =>
                        {
                            Co2.Text = co2Int.ToString();
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

            Task.Run(() =>
            {
                Alive = true;
                while (Alive)
                {
                    if (DateTime.Now.Subtract(LastReading).Seconds > 30)
                    {
                        if (mqttClient.Client.IsConnected)
                        {
                            try
                            {
                                LastReading = DateTime.Now;
                                mqttClient.PublishAsync($"blackbox/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/0/{SelectedBlackBox.mac}/set-continuousvalues", "on", false);
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

        private async void IdentifyButton_Click(object sender, EventArgs e)
        {
            if (mqttClient != null && mqttClient.Client != null && mqttClient.Client.IsConnected)
            {
                try
                {
                    await mqttClient.PublishAsync($"blackbox/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/0/{SelectedBlackBox.mac}/set-blinking", "on", false);
                    Log.Info("MQTT", "BLINKING SENT");
                }
                catch (Exception ex)
                {
                    Log.Error("MQTT", ex.Message);
                }
            }
        }

        private async void CalibrateWindButton_Click(object sender, EventArgs e)
        {
            Alive = false;
            if (mqttClient != null && mqttClient.Client != null)
            {
                try
                {
                    await mqttClient.PublishAsync($"blackbox/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/0/{SelectedBlackBox.mac}/set-continuousvalues", "off", false);
                    Log.Info("MQTT", "MESSAGE SENT STOP");
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

            BlackBoxWindCalibration dialog = new BlackBoxWindCalibration(SelectedBlackBox, SelectedRoom, OnResume);
            dialog.Show(Activity.SupportFragmentManager, null);
        }

        private async void CalibrateCO2Button_Click(object sender, EventArgs e)
        {
            Alive = false;
            if (mqttClient != null && mqttClient.Client != null)
            {
                try
                {
                    await mqttClient.PublishAsync($"blackbox/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/0/{SelectedBlackBox.mac}/set-continuousvalues", "off", false);
                    Log.Info("MQTT", "MESSAGE SENT STOP");
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

            BlackBoxCO2Calibration dialog = new BlackBoxCO2Calibration(SelectedBlackBox, SelectedRoom, OnResume);
            dialog.Show(Activity.SupportFragmentManager, null);
        }

        public async override void OnPause()
        {
            base.OnPause();
            Alive = false;
            if(mqttClient != null && mqttClient.Client != null)
            {
                try
                {
                    await mqttClient.PublishAsync($"blackbox/{SelectedRoom.center.idCenter}/{SelectedRoom.type.idRoomType}/{SelectedRoom.number}/0/{SelectedBlackBox.mac}/set-continuousvalues", "off", false);
                    Log.Info("MQTT", "MESSAGE SENT STOP");
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
