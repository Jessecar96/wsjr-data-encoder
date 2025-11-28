using CommandLine;
using JrEncoderLib.StarAttributes;
using MQTTnet;

namespace JrCommand
{
    internal class Program
    {
        private static IMqttClient? mqttClient;
        private static string[]? args;

        static async Task Main(string[] args)
        {
            Program.args = args;

            MqttClientFactory mqttFactory = new MqttClientFactory();
            mqttClient = mqttFactory.CreateMqttClient();
            MqttClientOptions? mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer("localhost").Build();

            // Connected event
            mqttClient.ConnectedAsync += MqttClientOnConnectedAsync;

            // Do the connection
            try
            {
                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to connect to MQTT broker: " + ex.Message);
            }
        }

        private static Task MqttClientOnConnectedAsync(MqttClientConnectedEventArgs arg)
        {
            // Parse command line args
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(ArgumentsParsed);
            return Task.CompletedTask;
        }

        private static void ArgumentsParsed(Options options)
        {
            // Finally we can work with everything

            if (options.Flavor != null)
            {
                mqttClient.PublishStringAsync("jrencoder/LF", options.Flavor);
                Console.WriteLine("Sent LF flavor: " + options.Flavor);
            } // flavor

            if (options.LDLStyle != null)
            {
                if (Enum.TryParse(options.LDLStyle, out LDLStyle style))
                {
                    mqttClient.PublishStringAsync("jrencoder/LDL/style", style.ToString());
                    Console.WriteLine("Sent LDL style: " + options.LDLStyle);
                }
                else
                {
                    Console.WriteLine("Failed to parse LDL style: " + options.LDLStyle);
                }
            } // ldl style

            if (options.LDLBackground != null)
            {
                bool enableBg = options.LDLBackground.Equals("1");
                mqttClient.PublishStringAsync("jrencoder/LDL/back", enableBg ? "1" : "0");
                Console.WriteLine(enableBg ? "LDL Background Enabled" : "LDL Background Disabled");
            } // ldl bg

            if (options.Warning != null)
            {
                mqttClient.PublishStringAsync("jrencoder/warning", options.Warning);
                Console.WriteLine("Warning sent");
            } // warning

            if (options.Beep)
            {
                mqttClient.PublishStringAsync("jrencoder/beep", "");
                Console.WriteLine("Beep sent");
            } // beep

            if (options.LoadConfig != null)
            {
                mqttClient.PublishStringAsync("jrencoder/load-config", options.LoadConfig);
                Console.WriteLine("Loading config " + options.LoadConfig);
            } // load config

            if (options.ReloadData)
            {
                mqttClient.PublishStringAsync("jrencoder/reload-data", "");
                Console.WriteLine("Data reload sent");
            } // data reload
        }
    }
}