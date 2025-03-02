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

    private Task MqttClientOnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
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
            
            case "jrencoder/warning":
                Program.ShowWxWarning(payloadStr, Address.All, _omcw);
                break;
        }

        return Task.CompletedTask;
    }

    private Task MqttClientOnConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        Console.WriteLine("[MQTTClient] Connected");
        return Task.CompletedTask;
    }
}