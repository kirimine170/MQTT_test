using System;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Formatter;
using MQTTnet.Client;
using System.Threading;
using MQTTnet.Client.Internal;
using MQTTnet.Packets;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Net.NetworkInformation;
using System.Reflection.Metadata;

namespace MQTT_test
{
    internal class Program
    {
        private const String MQTT_SERVER = "localhost";
        private const int MQTT_PORT = 1883;
        private const String TEST_TOPIC = "test/topic";
        private const String TEST_PAYLOAD = "Just Testing...";
        
        static void Main(string[] args)
        {
            Program program = new Program();
            program.Connect().Wait();
            program.Subscribe_Topic().Wait();
            program.Publish_Application_Message().Wait();
        }

        public async Task Connect()
        {
            MqttFactory mqttFactory = new MqttFactory();

            using IMqttClient mqttClient = mqttFactory.CreateMqttClient();

            MqttClientOptions mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(MQTT_SERVER, MQTT_PORT)
                .WithProtocolVersion(MqttProtocolVersion.V500)
                .Build();

            MqttClientConnectResult response = await mqttClient.ConnectAsync(mqttClientOptions);

            Console.WriteLine($"Client connect result: {response.ResultCode}");
        }

        public async Task Publish_Application_Message()
        {
            MqttFactory mqttFactory = new MqttFactory();

            using (IMqttClient mqttClient = mqttFactory.CreateMqttClient())
            {
                MqttClientOptions mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer(MQTT_SERVER, MQTT_PORT)
                    .WithProtocolVersion(MqttProtocolVersion.V500)
                    .Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                MqttApplicationMessage applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(TEST_TOPIC)
                    .WithPayload(TEST_PAYLOAD)
                    .Build();

                await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                await mqttClient.DisconnectAsync();

                Console.WriteLine("MQTT application message is published.");
            }
        }

        public async Task Subscribe_Topic()
        {
            MqttFactory mqttFactory = new MqttFactory();

            using (IMqttClient mqttClient = mqttFactory.CreateMqttClient())
            {
                MqttClientOptions mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer(MQTT_SERVER, MQTT_PORT)
                    .WithProtocolVersion(MqttProtocolVersion.V500)
                    .Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                MqttClientSubscribeOptions mqttSubscribeOptions = mqttFactory
                    .CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(new MqttTopicFilterBuilder().WithTopic(TEST_TOPIC).Build())
                    .Build();

                MqttClientSubscribeResult response = await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

                Console.WriteLine("MQTT client subscribed to topic.");

                foreach (MqttUserProperty mqttUserProperty in response.UserProperties)
                {
                    Console.WriteLine(mqttUserProperty.Name + ":" + mqttUserProperty.Value);
                }
                
                foreach (MqttClientSubscribeResultItem item in response.Items)
                {
                    Console.WriteLine(item.ResultCode);
                }

            }

        }

    }
}
