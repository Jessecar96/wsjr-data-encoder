using MQTTnet.Server;

namespace JrEncoder;

public class MQTTServer
{
    public async Task Run()
    {
        using MqttServer? mqttServer = new MqttServerFactory()
            .CreateMqttServer(new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()
                .Build());
        mqttServer.StartedAsync += args =>
        {
            Console.WriteLine("[MQTT] Server started");
            return Task.CompletedTask;
        };
        await mqttServer.StartAsync();

        // You have to do this to keep the server running heh
        while (true)
            await Task.Delay(1000);
    }
}