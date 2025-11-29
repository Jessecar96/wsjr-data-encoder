using System.Text;
using System.Text.Json;
using JrEncoderLib.StarAttributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JrEncoder;

public class WebServer(Config config, Flavors flavors, OMCW omcw)
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

        app.MapPost("/loadLocalPresentation", (string flavor) =>
        {
            // There is no loading on the jr!
            // so we do nothing :)

            // Return json response
            dynamic response = new
            {
                success = true,
                message = "Saved Config",
            };
            return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
        });

        app.MapPost("/runLocalPresentation", async (HttpContext context) =>
        {
            using StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8);
            string flavor = await reader.ReadToEndAsync();

            // Run that flavor in the background on a new task
            _ = Task.Run(() => Program.FlavorMan.RunFlavor(flavor));

            // Return json response
            dynamic response = new
            {
                success = true,
                message = "Here we go grandma!",
            };
            return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
        });

        app.MapPost("/cancelLocalPresentation", () =>
        {
            Program.FlavorMan.CancelLF();

            // Return json response
            dynamic response = new
            {
                success = true,
                message = "LF Cancelled",
            };
            return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
        });

        app.MapPost("/sendAlert", ([FromForm] string alertText, [FromForm] string alertType) =>
        {
            // Show warning
            WarningType type;
            try
            {
                type = Enum.Parse<WarningType>(alertType);
            }
            catch (Exception)
            {
                // Return json response
                return JsonSerializer.Serialize(new
                {
                    success = true,
                    message = "Invalid alert type",
                }, new JsonSerializerOptions { WriteIndented = true });
            }

            Program.ShowWxWarning(Util.WordWrapGeneric(alertText), type, Address.All, omcw);

            // Return json response
            dynamic response = new
            {
                success = true,
                message = "Alert Sent",
            };
            return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
        }).DisableAntiforgery();

        await app.RunAsync();
    }

    public void SetConfig(Config config)
    {
        _config = config;
    }
}