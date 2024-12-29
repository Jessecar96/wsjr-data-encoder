using System.Text.Json;
using JrEncoderLib;
using JrEncoderLib.DataTransmitter;
using JrEncoderLib.StarAttributes;
using Newtonsoft.Json;

namespace JrEncoder;

public class DataDownloader(Config config, DataTransmitter dataTransmitter, OMCW omcw)
{
    private Config _config = config;
    private DataTransmitter _dataTransmitter = dataTransmitter;
    private OMCW _omcw = omcw;

    public async Task Run()
    {
        Console.WriteLine("[DataDownloader] Running...");
        await UpdateCurrentConditions();
    }

    private async Task UpdateCurrentConditions()
    {
        bool firstRun = true;
        using PeriodicTimer timer = new(TimeSpan.FromMinutes(5));
        while (firstRun || await timer.WaitForNextTickAsync())
        {
            firstRun = false;

            foreach (Config.WeatherStar star in _config.Stars)
            {
                Console.WriteLine($"[DataDownloader] Updating current conditions for {star.LocationName}");

                // Make HTTP request
                HttpResponseMessage httpResponseMessage = await Util.HttpClient.GetAsync(
                    $"https://api.weather.com/v3/wx/observations/current?geocode={star.Location}&units=e&language=en-US&format=json&apiKey={_config.APIKey}");

                // Make sure the request was successful
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[DataDownloader] Failed to download current conditions for {star.LocationName}");
                    continue;
                }

                string responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                dynamic conditionsData = JsonConvert.DeserializeObject(responseBody);
                var jsonDocument = JsonDocument.Parse(responseBody);
                var conditionsData2 = jsonDocument.RootElement;

                // Build padding into strings 
                string tempStr = conditionsData2.Get<string>("temperature").PadLeft(4, ' ');
                string wcStr = conditionsData2.GetProperty("temperatureWindChill").GetRawText().PadLeft(3, ' ');
                string humStr = conditionsData2.GetProperty("relativeHumidity").GetRawText().PadLeft(4, ' ');
                string dewptStr = conditionsData2.GetProperty("temperatureDewPoint").GetRawText().PadLeft(3, ' ');
                string presStr = conditionsData2.GetProperty("pressureAltimeter").GetRawText().PadLeft(6, ' ');
                string windDir = conditionsData2.GetProperty("windDirectionCardinal").GetString().PadLeft(4, ' ');
                string windSpeedStr = conditionsData2.GetProperty("windSpeed").GetRawText().PadLeft(3, ' ');
                string visibStr = conditionsData2.GetProperty("visibility").GetRawText().PadLeft(4, ' ');
                string ceilingStr = conditionsData2.GetProperty("cloudCeiling").GetRawText().PadLeft(5, ' ');

                //string tempStr = conditionsData2.Raw("temperature").PadLeft(4, ' ');

                // Build page
                var testPage = new PageBuilder(50, Address.FromSwitches(star.Switches), _omcw)
                    .AddLine($"Conditions at {star.LocationName}")
                    .AddLine(conditionsData.wxPhraseLong.ToString())
                    .AddLine($"Temp:{tempStr}°F    Wind Chill:{wcStr}°F")
                    .AddLine($"Humidity:{humStr}%   Dewpoint:{dewptStr}°F")
                    .AddLine($"Barometric Pressure:{presStr} in.")
                    .AddLine($"Wind:{windDir}{windSpeedStr} MPH")
                    .AddLine($"Visib:{visibStr} mi. Ceiling:{ceilingStr} ft.")
                    .Build();
                
                _dataTransmitter.AddFrame(testPage);
                Console.WriteLine($"[DataDownloader] Page 50 sent");

                // Change to page 0 then 50
                omcw.TopPage(0).Commit();
                Thread.Sleep(100);
                omcw.TopPage(50).Commit();
            }
        }
    }
}