using System.Buffers;
using System.Text;
using System.Text.Json;
using JrEncoderLib.StarAttributes;
using MQTTnet;

namespace JrEncoder;

public class MQTTClient(OMCW omcw)
{
    public async Task Run()
    {
        MqttClientFactory mqttFactory = new MqttClientFactory();

        using IMqttClient? mqttClient = mqttFactory.CreateMqttClient();
        MqttClientOptions? mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer("localhost").Build();

        // Handle received messages
        mqttClient.ApplicationMessageReceivedAsync += MqttClientOnApplicationMessageReceivedAsync;

        // Event for connecting, just to log it
        mqttClient.ConnectedAsync += MqttClientOnConnectedAsync;

        await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

        MqttClientSubscribeOptions? mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
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
        string topic = arg.ApplicationMessage.Topic.ToLower();
        Logger.Debug("[MQTTClient] Topic: " + topic + " Payload: " + payloadStr);

        switch (topic)
        {
            case "jrencoder/lf":
                _ = Task.Run(() => Program.RunFlavor(payloadStr));
                break;
            
            case "jrencoder/timed-lf":
                try
                {
                    JsonDocument jsonDoc = JsonDocument.Parse(payloadStr);
                    string flavor = jsonDoc.RootElement.GetProperty("flavor").GetString() ?? "";
                    double time = jsonDoc.RootElement.GetProperty("time").GetDouble();
                    DateTimeOffset runTime = DateTimeOffset.FromUnixTimeSeconds((long)time);
                    Logger.Info($"[MQTTClient] flavor: {flavor} Run time: {runTime}");
                    _ = Task.Run(() => Program.RunFlavor(flavor, runTime));
                }
                catch (JsonException e)
                {
                    Logger.Error($"[MQTTClient] Unable to parse json: " + e);
                }
                catch (Exception e)
                {
                    Logger.Error($"[MQTTClient] Exception: " + e);
                }
                break;

            case "jrencoder/LDL/back":
                omcw.BottomSolid(payloadStr.Equals("1")).RegionSeparator(payloadStr.Equals("1")).Commit();
                break;

            case "jrencoder/LDL/style":
                if (Enum.TryParse(payloadStr, out LDLStyle style))
                    omcw.LDL(style).Commit();
                else
                    Logger.Error("[MQTTClient] Failed to parse LDL style: " + payloadStr);
                break;

            // Show a warning roll
            case "jrencoder/warning":
                Program.ShowWxWarning(payloadStr, WarningType.Warning, Address.All, omcw);
                break;

            // Show a warning roll
            case "jrencoder/advisory":
                Program.ShowWxWarning(payloadStr, WarningType.Advisory, Address.All, omcw);
                break;

            // Trigger the wx warning relay for 1 second
            case "jrencoder/beep":
                omcw.WxWarning(true).Commit();
                await Task.Delay(1000);
                omcw.WxWarning(false).Commit();
                break;

            case "jrencoder/load-config":
                await Program.LoadConfig(payloadStr);
                Logger.Info("[MQTTClient] Config file changed");
                await Program.downloader.UpdateAll();
                Logger.Info("[MQTTClient] All data reloaded");
                break;

            case "jrencoder/reload-data":
                await Program.downloader.UpdateAll();
                Logger.Info("[MQTTClient] All data reloaded");
                break;
        }
    }

    private Task MqttClientOnConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        Logger.Info("[MQTTClient] Connected");
        return Task.CompletedTask;
    }
}