using System.Text.Json;
using Microsoft.AspNetCore.Builder;

namespace JrEncoder;

public class WebServer(Config config, Flavors flavors)
{
    private Config _config = config;
    private Flavors _flavors = flavors;

    public async Task Run()
    {
        WebApplication app = WebApplication.Create();

        // Set port
        app.Urls.Add("http://*:5000");

        // Serve files from the wwwroot folder
        app.UseDefaultFiles();
        app.UseStaticFiles();

        // Server requests
        app.MapGet("/test", () => "This is the test page");
        app.MapGet("/getConfig", () =>
        {
            ConfigWebResponse response = new()
            {
                config = _config,
                flavors = _flavors
            };
            return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
        });
        app.MapPost("/setConfig", (ConfigWebResponse newConfig) =>
        {
            // Set the new config
            _config = newConfig.config;
            _config.Save();
            Console.WriteLine("Saved config file");
            
            // Set flavors
            _flavors = newConfig.flavors;
            _flavors.Save();
            Console.WriteLine("Saved flavors file");

            // Return json response
            dynamic response = new
            {
                success = true,
                message = "Saved Config",
            };
            return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
        });
        app.MapGet("/loadLocalPresentation", (string flavor) => { return "OK"; });
        app.MapGet("/runLocalPresentation", (string flavor) =>
        {
            _ = Task.Run(() => Program.RunFlavor(flavor));
            return "OK";
        });

        await app.RunAsync();
    }

    public void SetConfig(Config config)
    {
        _config = config;
    }
}