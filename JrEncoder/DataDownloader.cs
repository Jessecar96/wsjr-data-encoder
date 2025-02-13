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

    // Data cache
    private readonly Dictionary<string, HeadlinesResponse> _alertsCache = new();

    public async Task UpdateAll()
    {
        Console.WriteLine("[DataDownloader] Updating all records...");
        await UpdateAlerts();
        await UpdateCurrentConditions();
        await UpdateAlmanac();
        await UpdateForecast();
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

    private async Task UpdateAlerts()
    {
        foreach (Config.WeatherStar star in _config.Stars)
        {
            Console.WriteLine($"[DataDownloader] Updating alerts for {star.LocationName}");

            // Make HTTP request
            HttpResponseMessage httpResponseMessage =
                await Util.HttpClient.GetAsync($"https://api.weather.com/v3/alerts/headlines?geocode={star.Location}&format=json&language=en-US&apiKey={_config.APIKey}");

            // Make sure the request was successful
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                Console.WriteLine($"[DataDownloader] Failed to download alerts for {star.LocationName}");
                continue;
            }

            string responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
            HeadlinesResponse? alertData = JsonConvert.DeserializeObject<HeadlinesResponse>(responseBody);

            if (alertData == null)
            {
                Console.WriteLine($"[DataDownloader] No alerts for {star.LocationName}");
                continue;
            }

            // Save locally
            _alertsCache[star.Location] = alertData;
        }
    }

    private async Task UpdateCurrentConditions()
    {
        foreach (Config.WeatherStar star in _config.Stars)
        {
            Console.WriteLine($"[DataDownloader] Updating current conditions for {star.LocationName}");

            // Make HTTP request
            HttpResponseMessage httpResponseMessage =
                await Util.HttpClient.GetAsync($"https://api.weather.com/v3/wx/observations/current?geocode={star.Location}&units=e&language=en-US&format=json&apiKey={_config.APIKey}");

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
            string tempStr = conditionsData.Temperature.ToString().PadLeft(4);
            string wcStr = conditionsData.TemperatureWindChill.ToString().PadLeft(3);
            string humStr = conditionsData.RelativeHumidity.ToString().PadLeft(4);
            string dewptStr = conditionsData.TemperatureDewPoint.ToString().PadLeft(3);
            string presStr = conditionsData.PressureAltimeter.ToString().PadLeft(6);
            string windDir = conditionsData.WindDirectionCardinal.PadLeft(4);
            string windSpeedStr = conditionsData.WindSpeed.ToString().PadLeft(3);
            string visibStr = conditionsData.Visibility.ToString().PadLeft(4);
            string ceilingStr = conditionsData.CloudCeiling == null ? " Unlimited" : ": " + (conditionsData.CloudCeiling + " ft.").PadLeft(5);

            // Build page
            DataFrame[] ccPage = new PageBuilder((int)Page.CurrentConditions, Address.FromSwitches(star.Switches), _omcw)
                .AddLine($"Conditions at {star.LocationName}")
                .AddLine(conditionsData.WxPhraseLong)
                .AddLine($"Temp:{tempStr}°F    Wind Chill:{wcStr}°F")
                .AddLine($"Humidity:{humStr}%   Dewpoint:{dewptStr}°F")
                .AddLine($"Barometric Pressure:{presStr} in.")
                .AddLine($"Wind:{windDir}{windSpeedStr} MPH")
                .AddLine($"Visib:{visibStr} mi. Ceiling{ceilingStr} ft.")
                .Build();

            _dataTransmitter.AddFrame(ccPage);
            Console.WriteLine($"[DataDownloader] Page {(int)Page.CurrentConditions} sent");
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
                Console.WriteLine($"[DataDownloader] Failed to download daily almanac for {star.LocationName}");
                continue;
            }

            string responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
            Almanac? dailyAlmanacData = JsonConvert.DeserializeObject<Almanac>(responseBody);

            if (dailyAlmanacData == null)
            {
                Console.WriteLine($"[DataDownloader] Failed to download daily almanac for {star.LocationName}");
                continue;
            }

            // Same but for monthly almanac

            // Make HTTP request
            HttpResponseMessage httpResponseMessage2 =
                await Util.HttpClient.GetAsync($"https://api.weather.com/v3/wx/almanac/monthly/1month?geocode={star.Location}&format=json&units=e&month={currentMonth}&apiKey={_config.APIKey}");

            // Make sure the request was successful
            if (!httpResponseMessage2.IsSuccessStatusCode)
            {
                Console.WriteLine($"[DataDownloader] Failed to download monthly almanac for {star.LocationName}");
                continue;
            }

            string responseBody2 = await httpResponseMessage2.Content.ReadAsStringAsync();
            Almanac? monthlyAlmanacData = JsonConvert.DeserializeObject<Almanac>(responseBody2);

            if (monthlyAlmanacData == null)
            {
                Console.WriteLine($"[DataDownloader] Failed to download monthly almanac for {star.LocationName}");
                continue;
            }

            // Build padding into strings
            string todayDayName = DateTime.Today.ToString("dddd");
            string tomorrowDayName = DateTime.Today.AddDays(1).ToString("dddd");
            string monthName = DateTime.Today.ToString("MMMM");
            // When it's Tuesday, Wednesday needs to be moved over 1 character left
            int leftDayPadding = DateTime.Today.DayOfWeek == DayOfWeek.Tuesday ? 9 : 10;

            string precipLine = "";
            if (monthlyAlmanacData.PrecipitationAverage[0] != null)
            {
                precipLine = $"Normal {monthName} Precip".PadRight(24, ' ') +
                             $"{monthlyAlmanacData.PrecipitationAverage[0]}in".PadLeft(7, ' ');
            }

            // Get sunset and sunrise
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
            DataFrame[] almanacPage = new PageBuilder((int)Page.Almanac, Address.FromSwitches(star.Switches), _omcw)
                .AddLine("  The Weather Channel Almanac   ")
                .AddLine("", smallHeight)
                .AddLine($"              {todayDayName.PadRight(leftDayPadding)}{tomorrowDayName}")
                .AddLine("Sunrise       " + cToday.CelestialInfo.SunRise?.ToLocalTime().ToString("h:mm tt") + "   " + cTomorrow.CelestialInfo.SunRise?.ToLocalTime().ToString("h:mm tt"))
                .AddLine("Sunset        " + cToday.CelestialInfo.SunSet?.ToLocalTime().ToString("h:mm tt") + "   " + cTomorrow.CelestialInfo.SunSet?.ToLocalTime().ToString("h:mm tt"))
                .AddLine($"Normal Low     {dailyAlmanacData.TemperatureAverageMin[0].ToString(),3} ?F    {dailyAlmanacData.TemperatureAverageMin[1].ToString(),3} ?F")
                .AddLine($"Normal High    {dailyAlmanacData.TemperatureAverageMax[0].ToString(),3} ?F    {dailyAlmanacData.TemperatureAverageMax[1].ToString(),3} ?F")
                .AddLine("")
                .AddLine(precipLine)
                .Build();

            _dataTransmitter.AddFrame(almanacPage);
            Console.WriteLine($"[DataDownloader] Page {(int)Page.Almanac} sent");
        }
    }

    private async Task UpdateForecast()
    {
        foreach (Config.WeatherStar star in _config.Stars)
        {
            Console.WriteLine($"[DataDownloader] Updating forecast for {star.LocationName}");

            // Make HTTP request
            HttpResponseMessage httpResponseMessage =
                await Util.HttpClient.GetAsync($"https://api.weather.com/v3/wx/forecast/daily/3day?geocode={star.Location}&format=json&units=e&language=en-US&apiKey={_config.APIKey}");

            // Make sure the request was successful
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                Console.WriteLine($"[DataDownloader] Failed to download forecast for {star.LocationName}");
                continue;
            }

            string responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
            Forecast? forecastData = JsonConvert.DeserializeObject<Forecast>(responseBody);

            if (forecastData == null)
            {
                Console.WriteLine($"[DataDownloader] Failed to download forecast for {star.LocationName}");
                continue;
            }

            TextLineAttributes smallHeight = new()
            {
                Color = Color.Blue,
                Border = true,
                Width = 0,
                Height = 0
            };
            
            // List to hold lines that overflow onto the next page
            List<string> overflowLines = new();

            // Loop over daypart items
            int curForecastPage = 0;
            for (int i = 0; i < forecastData.Daypart[0].DaypartName.Count; i++)
            {
                // Make sure this isn't null
                if (forecastData.Daypart[0].DaypartName[i] == null)
                    continue;

                // Limit to 3 pages
                if (curForecastPage >= 3)
                    break;

                // List that will hold the lines of text for the forecast section
                List<string> narrativeLines = new();

                // Check for any headlines to show
                // Only do this for page 0
                if (curForecastPage == 0 && _alertsCache.ContainsKey(star.Location))
                {
                    // Find the saved alerts for this location
                    HeadlinesResponse headlinesResponseData = _alertsCache[star.Location];
                    foreach (HeadlinesResponse.Alert alert in headlinesResponseData.Alerts)
                    {
                        // Skip any that we don't want to show as a headline
                        if (!alert.ShowAsHeadline()) continue;
                        DateTime endTime = DateTimeOffset.FromUnixTimeSeconds(alert.EndTimeUTC).LocalDateTime;
                        narrativeLines.Add("*" + Util.CenterString(alert.EventDescription, 30) + "*");
                        narrativeLines.Add("*" + Util.CenterString("Until " + endTime.ToString("htt ddd"), 30) + "*");
                    }
                    
                    // We added some alerts, so add a line of padding before the forecast
                    if (narrativeLines.Count != 0)
                        narrativeLines.Add("");
                }
                
                // Add any overflow lines from the previous page
                if (overflowLines.Count != 0)
                {
                    // Add overflowed lines to this page
                    narrativeLines.AddRange(overflowLines);
                    
                    // Clear the list for this page to overflow any more lines
                    overflowLines.Clear();
                }

                // Build narrative and word wrap it
                string narrative = forecastData.Daypart[0].DaypartName[i] + "..." + forecastData.Daypart[0].Narrative[i];
                narrative = Util.RemoveUnits(narrative);
                narrativeLines.AddRange(Util.WordWrap(narrative, 31));

                // Anything over 7 lines (total 9 lines with the title) needs to overflow to the next page
                if (narrativeLines.Count > 7)
                {
                    overflowLines = narrativeLines.Slice(7, narrativeLines.Count - 7);
                    narrativeLines.RemoveRange(7, narrativeLines.Count - 7);
                }

                // Build page
                int actualPageNum = (int)Page.Forecast1 + curForecastPage;
                PageBuilder pageBuilder = new PageBuilder(actualPageNum, Address.FromSwitches(star.Switches), _omcw)
                    .AddLine("        Your TWC Forecast", smallHeight)
                    .AddLine("");

                // Add lines to the page
                foreach (string line in narrativeLines)
                    pageBuilder.AddLine(line);

                _dataTransmitter.AddFrame(pageBuilder.Build());
                Console.WriteLine($"[DataDownloader] Page {actualPageNum} sent");

                curForecastPage++;
            }
        }
    }
}