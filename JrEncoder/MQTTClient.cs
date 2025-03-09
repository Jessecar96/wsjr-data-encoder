using System.Buffers;
using System.Text;
using JrEncoderLib.StarAttributes;
using MQTTnet;

namespace JrEncoder;

public class MQTTClient(OMCW _omcw)
{
    public async Task Run()
    {
        var mqttFactory = new MqttClientFactory();

        using var mqttClient = mqttFactory.CreateMqttClient();
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer("localhost").Build();

        // Handle received messages
        mqttClient.ApplicationMessageReceivedAsync += MqttClientOnApplicationMessageReceivedAsync;

        // Event for connecting, just to log it
        mqttClient.ConnectedAsync += MqttClientOnConnectedAsync;

        await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

        var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter("jrencoder/#")
            .Build();

        await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

        // You have to do this to keep this running...
        while (true)
            await Task.Delay(1000);
    }

    private async Task MqttClientOnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
    {
        ReadOnlySequence<byte> payload = arg.ApplicationMessage.Payload;
        string payloadStr = Encoding.UTF8.GetString(payload.ToArray());
        string topic = arg.ApplicationMessage.Topic;
        Console.WriteLine("[MQTTClient] Topic: " + topic + " Payload: " + payloadStr);

        switch (topic)
        {
            case "jrencoder/LF":
                _ = Task.Run(() => Program.RunFlavor(payloadStr));
                break;

            case "jrencoder/LDL/back":
                _omcw.BottomSolid(payloadStr.Equals("1")).RegionSeparator(payloadStr.Equals("1")).Commit();
                break;

            case "jrencoder/LDL/style":
                if (Enum.TryParse(payloadStr, out LDLStyle style))
                    _omcw.LDL(style).Commit();
                else
                    Console.WriteLine("[MQTTClient] Failed to parse LDL style: " + payloadStr);
                break;

            // Show a warning roll
            case "jrencoder/warning":
                Program.ShowWxWarning(payloadStr, WarningType.Warning, Address.All, _omcw);
                break;
            
            // Show a warning roll
            case "jrencoder/advisory":
                Program.ShowWxWarning(payloadStr, WarningType.Advisory, Address.All, _omcw);
                break;

            // Trigger the wx warning relay for 1 second
            case "jrencoder/beep":
                _omcw.WxWarning(true).Commit();
                await Task.Delay(1000);
                _omcw.WxWarning(false).Commit();
                break;

            case "jrencoder/load-config":
                Program.LoadConfig(payloadStr);
                break;

            case "jrencoder/reload-data":
                await Program.downloader.UpdateAll();
                Console.WriteLine("[MQTTClient] All data reloaded");
                break;
        }
    }

    private Task MqttClientOnConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        Console.WriteLine("[MQTTClient] Connected");
        return Task.CompletedTask;
    }
}