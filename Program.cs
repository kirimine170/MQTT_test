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
using System.Security.Authentication;
using Microsoft.Extensions.Configuration;

namespace MQTT_test
{
    internal class Program
    {
        private readonly IConfiguration _configuration;
        // 共通設定
        private string MQTT_SERVER => _configuration["Mqtt:Server"];
        private int MQTT_PORT => int.Parse(_configuration["Mqtt:Port"]);
        private string TEST_TOPIC => _configuration["Test:Topic"];
        private string TEST_PAYLOAD => _configuration["Test:Payload"];

        // 認証情報セット A
        private string MQTT_USERNAME_A => _configuration["Mqtt:A:Username"];
        private string MQTT_PASSWORD_A => _configuration["Mqtt:A:Password"];
        private string MQTT_CLIENT_ID_A => _configuration["Mqtt:A:ClientId"];

        // 認証情報セット B
        private string MQTT_USERNAME_B => _configuration["Mqtt:B:Username"];
        private string MQTT_PASSWORD_B => _configuration["Mqtt:B:Password"];
        private string MQTT_CLIENT_ID_B => _configuration["Mqtt:B:ClientId"];

        public Program(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        static async Task Main(string[] args)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddJsonFile("config.json", optional: true)
                .AddEnvironmentVariables();

            IConfiguration configuration = builder.Build();
            Program program = new Program(configuration);
            // program.Connect().Wait();
            // program.Subscribe_Topic().Wait();
            // program.Publish_Application_Message().Wait();
            Task subscriberTask = program.Subscribe_Topic();
            await Task.Delay(1000);
            await program.Publish_Application_Message();
            await subscriberTask;
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
                    .WithCredentials(MQTT_USERNAME_A, MQTT_PASSWORD_A)
                    .WithTlsOptions
                    (
                        o =>
                        {
                            o.WithCertificateValidationHandler(_ => true);
                            o.WithSslProtocols(SslProtocols.Tls12);
                        }

                    )
                    .WithProtocolVersion(MqttProtocolVersion.V500)
                    .WithClientId(MQTT_CLIENT_ID_A)
                    .Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                MqttApplicationMessage applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(TEST_TOPIC)
                    .WithPayload(TEST_PAYLOAD)
                    .Build();

                await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                Console.WriteLine("MQTT application message is published.");

                await mqttClient.DisconnectAsync();
            }
        }

        public async Task Subscribe_Topic()
        {
            MqttFactory mqttFactory = new MqttFactory();

            IMqttClient mqttClient = mqttFactory.CreateMqttClient();

            MqttClientOptions mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(MQTT_SERVER, MQTT_PORT)
                .WithCredentials(MQTT_USERNAME_B, MQTT_PASSWORD_B)
                .WithTlsOptions
                (
                    o =>
                    {
                        o.WithCertificateValidationHandler(_ => true);
                        o.WithSslProtocols(SslProtocols.Tls12);
                    }

                )
                .WithProtocolVersion(MqttProtocolVersion.V500)
                .WithClientId(MQTT_CLIENT_ID_B)
                .Build();

            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                string topic = e.ApplicationMessage.Topic;
                string payload = e.ApplicationMessage.ConvertPayloadToString();
                Console.WriteLine($"Received message on topic: {topic}");
                Console.WriteLine($"Payload: {payload}");
                int id = System.Threading.Thread.CurrentThread.ManagedThreadId;
                Console.WriteLine("ThreadID : " + id);

                return Task.CompletedTask;
            };

            MqttClientSubscribeOptions mqttSubscribeOptions = mqttFactory
                .CreateSubscribeOptionsBuilder()
                .WithTopicFilter(new MqttTopicFilterBuilder()
                    .WithTopic(TEST_TOPIC)
                    .Build())
                .Build();

            MqttClientSubscribeResult response = await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

            Console.WriteLine("MQTT client subscribed to topic.");
            Console.WriteLine("Waiting for messages... Press Enter to exit.");

            Console.ReadLine();

            await mqttClient.DisconnectAsync();

        }

    }
}
