using System;
using System.Threading.Tasks;
using Android.Util;
using AVG_Scale_Installer.Tools;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;

namespace AVG_Scale_Installer
{
    public class MqttClient
    {
        public ManagedMqttClient Client;
        public const int PORT = 1884;

        public MqttClient()
        {
            Client = (ManagedMqttClient)new MqttFactory().CreateManagedMqttClient();
        }

        public async Task ConnectAsync()
        {
            string clientId = Guid.NewGuid().ToString();
            string mqttURI = Data.MyAddress.ToMqtt();
            //string mqttURI = "dashboards.aviagensau.com";
            string mqttUser = "aviartec";
            string mqttPassword = "avg_aviartec";
            bool mqttSecure = false;

            var messageBuilder = new MqttClientOptionsBuilder()
                .WithClientId(clientId)
                .WithCredentials(mqttUser, mqttPassword)
                .WithTcpServer(mqttURI, PORT)
                .WithCleanSession();

            var options = mqttSecure ?
                messageBuilder.WithTls().Build() : messageBuilder.Build();

            var managedOptions = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(options)
                .Build();

            await Client.StartAsync(managedOptions);
        }

        public async Task PublishAsync(string topic, string payload, bool retainFlag = true, int qos = 1)
        {
            await Client.PublishAsync(new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)qos)
            .WithRetainFlag(retainFlag)
            .Build());

        }

        public async Task SubscribeAsync(string topic, int qos = 1)
        {
            await Client.SubscribeAsync(new MqttTopicFilterBuilder()
            .WithTopic(topic)
            .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)qos)
            .Build());
        }
    }
}
