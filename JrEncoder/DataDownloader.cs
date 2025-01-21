using CoordinateSharp;
using JrEncoder.Schema.TWC;
using JrEncoderLib;
using JrEncoderLib.DataTransmitter;
using JrEncoderLib.Frames;
using JrEncoderLib.StarAttributes;
using Newtonsoft.Json;

namespace JrEncoder;

public class DataDownloader(Config config, DataTransmitter dataTransmitter, OMCW omcw)
{
    private Config _config = config;
    private DataTransmitter _dataTransmitter = dataTransmitter;
    private OMCW _omcw = omcw;

    public async Task UpdateAll()
    {
        Console.WriteLine("[DataDownloader] Updating all records...");
        await UpdateCurrentConditions();
        await UpdateAlmanac();
    }

    public async Task Run()
    {
        Console.WriteLine("[DataDownloader] Running...");
        using PeriodicTimer timer = new(TimeSpan.FromMinutes(5));
        while (await timer.WaitForNextTickAsync())
        {
            await UpdateCurrentConditions();
        }
    }

    private async Task UpdateCurrentConditions()
    {
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
            Observations? conditionsData = JsonConvert.DeserializeObject<Observations>(responseBody);

            if (conditionsData == null)
            {
                Console.WriteLine($"[DataDownloader] Failed to download current conditions for {star.LocationName}");
                continue;
            }

            // Build padding into strings 
            string tempStr = conditionsData.Temperature.ToString().PadLeft(4, ' ');
            string wcStr = conditionsData.TemperatureWindChill.ToString().PadLeft(3, ' ');
            string humStr = conditionsData.RelativeHumidity.ToString().PadLeft(4, ' ');
            string dewptStr = conditionsData.TemperatureDewPoint.ToString().PadLeft(3, ' ');
            string presStr = conditionsData.PressureAltimeter.ToString().PadLeft(6, ' ');
            string windDir = conditionsData.WindDirectionCardinal.PadLeft(4, ' ');
            string windSpeedStr = conditionsData.WindSpeed.ToString().PadLeft(3, ' ');
            string visibStr = conditionsData.Visibility.ToString().PadLeft(4, ' ');
            string ceilingStr = conditionsData.CloudCeiling == null ? " Unlimited" : ": " + (conditionsData.CloudCeiling + " ft.").PadLeft(5, ' ');

            // Build page
            DataFrame[] testPage = new PageBuilder(50, Address.FromSwitches(star.Switches), _omcw)
                .AddLine($"Conditions at {star.LocationName}")
                .AddLine(conditionsData.WxPhraseLong)
                .AddLine($"Temp:{tempStr}°F    Wind Chill:{wcStr}°F")
                .AddLine($"Humidity:{humStr}%   Dewpoint:{dewptStr}°F")
                .AddLine($"Barometric Pressure:{presStr} in.")
                .AddLine($"Wind:{windDir}{windSpeedStr} MPH")
                .AddLine($"Visib:{visibStr} mi. Ceiling{ceilingStr} ft.")
                .Build();

            _dataTransmitter.AddFrame(testPage);
            Console.WriteLine($"[DataDownloader] Page 50 sent");
        }
    }

    private async Task UpdateAlmanac()
    {
        foreach (Config.WeatherStar star in _config.Stars)
        {
            Console.WriteLine($"[DataDownloader] Updating almanac for {star.LocationName}");

            int currentDay = DateTime.Now.Day;
            int currentMonth = DateTime.Now.Month;

            // Make HTTP request
            HttpResponseMessage httpResponseMessage =
                await Util.HttpClient.GetAsync($"https://api.weather.com/v3/wx/almanac/daily/5day?geocode={star.Location}&format=json&units=e&startDay={currentDay}&startMonth={currentMonth}&apiKey={_config.APIKey}");

            // Make sure the request was successful
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                Console.WriteLine($"[DataDownloader] Failed to download almanac for {star.LocationName}");
                continue;
            }

            string responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
            Almanac? almanacData = JsonConvert.DeserializeObject<Almanac>(responseBody);

            if (almanacData == null)
            {
                Console.WriteLine($"[DataDownloader] Failed to download almanac for {star.LocationName}");
                continue;
            }

            // Build padding into strings
            string todayDayName = DateTime.Today.ToString("dddd");
            string tomorrowDayName = DateTime.Today.AddDays(1).ToString("dddd");
            string monthName = DateTime.Today.ToString("MMMM");
            // When it's Tuesday, Wednesday needs to be moved over 1 character left
            int leftDayPadding = DateTime.Today.DayOfWeek == DayOfWeek.Tuesday ? 9 : 10;

            string precipLine = "";
            if (almanacData.PrecipitationAverage[0] != null)
            {
                precipLine = $"Normal {monthName} Precip".PadRight(24, ' ') +
                             $"{almanacData.PrecipitationAverage[0]}in".PadLeft(7, ' ');
            }

            // Get sunset and sunrise
            Console.WriteLine(star.GetLat() + "," + star.GetLon());
            Coordinate cToday = new(star.GetLat(), star.GetLon(), DateTime.Now);
            Coordinate cTomorrow = new(star.GetLat(), star.GetLon(), DateTime.Now.AddDays(1));

            TextLineAttributes smallHeight = new()
            {
                Color = Color.Blue,
                Border = true,
                Width = 0,
                Height = 0
            };

            // Build page
            DataFrame[] testPage = new PageBuilder(51, Address.FromSwitches(star.Switches), _omcw)
                .AddLine("  The Weather Channel Almanac   ")
                .AddLine("", smallHeight)
                .AddLine($"            {todayDayName.PadRight(-leftDayPadding)}{tomorrowDayName}")
                .AddLine("Sunrise       " + cToday.CelestialInfo.SunRise?.ToLocalTime().ToString("h:mm tt") + "   " + cTomorrow.CelestialInfo.SunRise?.ToLocalTime().ToString("h:mm tt"))
                .AddLine("Sunset        " + cToday.CelestialInfo.SunSet?.ToLocalTime().ToString("h:mm tt") + "   " + cTomorrow.CelestialInfo.SunSet?.ToLocalTime().ToString("h:mm tt"))
                .AddLine($"Normal Low     {almanacData.TemperatureAverageMin[0].ToString(),3} ?F    {almanacData.TemperatureAverageMin[1].ToString(),3} ?F")
                .AddLine($"Normal High    {almanacData.TemperatureAverageMax[0].ToString(),3} ?F    {almanacData.TemperatureAverageMax[1].ToString(),3} ?F")
                .AddLine("")
                .AddLine(precipLine)
                .Build();

            _dataTransmitter.AddFrame(testPage);
            Console.WriteLine($"[DataDownloader] Page 51 sent");
        }
    }
}