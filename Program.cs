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

namespace MQTT_test
{
    internal class Program
    {
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
                .WithTcpServer("localhost")
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
                    .WithTcpServer("localhost")
                    .WithProtocolVersion(MqttProtocolVersion.V500)
                    .Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                MqttApplicationMessage applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("samples/temperature/living_room")
                    .WithPayload("19.5")
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
                    .WithTcpServer("localhost")
                    .WithProtocolVersion(MqttProtocolVersion.V500)
                    .Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                MqttClientSubscribeOptions mqttSubscribeOptions = mqttFactory
                    .CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(new MqttTopicFilterBuilder().WithTopic("sample/temperature/#").Build())
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
