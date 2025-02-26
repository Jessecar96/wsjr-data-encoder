﻿using System.Text.Json;
using CoordinateSharp;
using JrEncoder.Schema.TWC;
using JrEncoderLib;
using JrEncoderLib.DataTransmitter;
using JrEncoderLib.Frames;
using JrEncoderLib.StarAttributes;

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

    public void Run()
    {
        Console.WriteLine("[DataDownloader] Running...");
        _ = Task.Run(async () =>
        {
            // Update alerts every 2 minutes
            using PeriodicTimer timer = new(TimeSpan.FromMinutes(2));
            while (await timer.WaitForNextTickAsync())
                await UpdateAlerts();
        });
        _ = Task.Run(async () =>
        {
            // Update current conditions every 5 minutes
            using PeriodicTimer timer = new(TimeSpan.FromMinutes(5));
            while (await timer.WaitForNextTickAsync())
                await UpdateCurrentConditions();
        });
        _ = Task.Run(async () =>
        {
            // Update forecast every 30 minutes
            using PeriodicTimer timer = new(TimeSpan.FromMinutes(30));
            while (await timer.WaitForNextTickAsync())
                await UpdateForecast();
        });
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

            if (string.IsNullOrEmpty(responseBody))
            {
                Console.WriteLine($"[DataDownloader] No alerts for {star.LocationName}");
                continue;
            }

            HeadlinesResponse? alertData = JsonSerializer.Deserialize<HeadlinesResponse>(responseBody);

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
        TextLineAttributes smallHeight = new()
        {
            Color = Color.Blue,
            Border = true,
            Width = 0,
            Height = 0
        };

        foreach (Config.WeatherStar star in _config.Stars)
        {
            Console.WriteLine($"[DataDownloader] Updating current conditions for {star.LocationName}");

            // Build list of locations to get current conditions for
            List<string> locations = [];

            // Add primary location
            locations.Add(star.Location);

            // Add nearby cities
            locations.AddRange(star.NearbyCities?.Geocodes.Where(str => !string.IsNullOrEmpty(str)) ?? []);
            locations.AddRange(star.RegionalCities?.Geocodes.Where(str => !string.IsNullOrEmpty(str)) ?? []);

            // Make HTTP request
            string locationsString = string.Join(';', locations);
            HttpResponseMessage httpResponseMessage =
                await Util.HttpClient.GetAsync($"https://api.weather.com/v3/aggcommon/v3-wx-observations-current?geocodes={locationsString}&language=en-US&format=json&units=e&apiKey={_config.APIKey}");

            // Make sure the request was successful
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                Console.WriteLine("[DataDownloader] Failed to download current conditions");
                SendCcNoReportPage(star);
                SendObsNoReportPage(star);
                continue;
            }

            // Parse response as JSON
            string responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
            List<AggregateResponse>? conditionsDatas = JsonSerializer.Deserialize<List<AggregateResponse>>(responseBody);

            // Make sure conditions didn't parse as null
            if (conditionsDatas == null)
            {
                Console.WriteLine("[DataDownloader] Failed to download current conditions");
                SendCcNoReportPage(star);
                SendObsNoReportPage(star);
                continue;
            }

            // // 
            // Primary location current conditions
            // //

            // Primary location conditions
            ObservationsResponse? conditionsData = conditionsDatas.FirstOrDefault(item => item.Id == star.Location)?.ObservationsResponse;

            if (conditionsData == null)
            {
                Console.WriteLine($"[DataDownloader] Failed to download current conditions for {star.LocationName}");
                SendCcNoReportPage(star);
            }
            else
            {
                // Build padding into strings
                string tempStr = (conditionsData.Temperature.ToString() ?? "").PadLeft(4);
                string wcStr = (conditionsData.TemperatureWindChill.ToString() ?? "").PadLeft(3);
                string humStr = (conditionsData.RelativeHumidity.ToString() ?? "").PadLeft(4);
                string dewptStr = (conditionsData.TemperatureDewPoint.ToString() ?? "").PadLeft(3);
                string presStr = (conditionsData.PressureAltimeter.ToString() ?? "").PadLeft(6);
                string windDir = (conditionsData.WindDirectionCardinal ?? "").PadLeft(4);
                string windSpeedStr = (conditionsData.WindSpeed.ToString() ?? "").PadLeft(3);
                string visibStr = (conditionsData.Visibility.ToString() ?? "").PadLeft(4);
                string ceilingStr = conditionsData.CloudCeiling == null ? " Unlimited" : ": " + (conditionsData.CloudCeiling + " ft.").PadLeft(5);

                // This page gets sent to both CC and LDL (they are two different pages)
                foreach (Page pageNum in new[] { Page.CurrentConditions, Page.LDL })
                {
                    // Build page
                    DataFrame[] ccPage = new PageBuilder((int)pageNum, Address.FromSwitches(star.Switches), _omcw)
                        .AddLine($"Conditions at {star.LocationName}")
                        .AddLine(conditionsData.WxPhraseLong ?? "")
                        .AddLine($"Temp:{tempStr}°F    Wind Chill:{wcStr}°F")
                        .AddLine($"Humidity:{humStr}%   Dewpoint:{dewptStr}°F")
                        .AddLine($"Barometric Pressure:{presStr} in.")
                        .AddLine($"Wind:{windDir}{windSpeedStr} MPH")
                        .AddLine($"Visib:{visibStr} mi. Ceiling{ceilingStr} ft.")
                        .Build();
                    _dataTransmitter.AddFrame(ccPage);
                    Console.WriteLine($"[DataDownloader] Page {(int)pageNum} sent");
                }
            } // end conditionsData null check

            // // 
            // Latest Observations
            // //

            PageBuilder nearbyObs = new PageBuilder((int)Page.LatestObservations, Address.FromSwitches(star.Switches), _omcw);
            nearbyObs.AddLine("   Latest Hourly Observations");
            nearbyObs.AddLine("LOCATION       ?F WEATHER   WIND", smallHeight);

            for (int i = 0; i < star.NearbyCities?.Locations.Count; i++)
            {
                if (i > 6) break; // Only 7 cities are allowed
                string locationName = star.NearbyCities.GetLocationName(i);
                string geocode = star.NearbyCities.Geocodes.ElementAtOrDefault(i) ?? "";
                conditionsData = conditionsDatas.FirstOrDefault(item => item.Id == geocode)?.ObservationsResponse;

                if (conditionsData == null)
                {
                    nearbyObs.AddLine($"{locationName,-14}    No Report");
                }
                else
                {
                    string temp = (conditionsData.Temperature.ToString() ?? "").PadLeft(3);
                    string cond = (conditionsData.WxPhraseShort ?? "").PadRight(10);
                    string windDir = conditionsData.WindDirectionCardinal ?? "";
                    string windSpeed = conditionsData.WindSpeed.ToString() ?? "";
                    string windAndSpeed = Util.FormatWindAndSpeed(windDir, windSpeed);
                    nearbyObs.AddLine($"{locationName,-14}{temp} {cond}{windAndSpeed}");
                }
            }

            _dataTransmitter.AddFrame(nearbyObs.Build());
            Console.WriteLine($"[DataDownloader] Page {(int)Page.LatestObservations} sent");

            // // 
            // Regional Observations
            // //

            PageBuilder regionalObs = new PageBuilder((int)Page.RegionalObservations, Address.FromSwitches(star.Switches), _omcw);
            regionalObs.AddLine("  Conditions Across The Region");
            regionalObs.AddLine("CITY                WEATHER   ?F", smallHeight);

            for (int i = 0; i < star.RegionalCities?.Locations.Count; i++)
            {
                if (i > 6) break; // Only 7 cities are allowed
                string locationName = star.RegionalCities.GetLocationName(i, 19);
                string geocode = star.RegionalCities.Geocodes.ElementAtOrDefault(i) ?? "";
                conditionsData = conditionsDatas.FirstOrDefault(item => item.Id == geocode)?.ObservationsResponse;

                if (conditionsData == null)
                {
                    regionalObs.AddLine($"{locationName,-19} No Report");
                }
                else
                {
                    string temp = (conditionsData.Temperature.ToString() ?? "").PadLeft(3);
                    string cond = (conditionsData.WxPhraseShort ?? "").PadRight(9);
                    regionalObs.AddLine($"{locationName,-19} {cond}{temp}");
                }
            }

            _dataTransmitter.AddFrame(regionalObs.Build());
            Console.WriteLine($"[DataDownloader] Page {(int)Page.RegionalObservations} sent");
        }
    }

    private void SendCcNoReportPage(Config.WeatherStar star)
    {
        DataFrame[] ccPage = new PageBuilder((int)Page.CurrentConditions, Address.FromSwitches(star.Switches), _omcw)
            .AddLine($"Conditions at {star.LocationName}")
            .AddLine("No Current Report")
            .Build();
        _dataTransmitter.AddFrame(ccPage);
        Console.WriteLine($"[DataDownloader] Page {(int)Page.CurrentConditions} sent");
    }

    private void SendObsNoReportPage(Config.WeatherStar star)
    {
        PageBuilder nearbyObs = new PageBuilder((int)Page.LatestObservations, Address.FromSwitches(star.Switches), _omcw);
        nearbyObs.AddLine("   Latest Hourly Observations");
        nearbyObs.AddLine("LOCATION       ?F WEATHER   WIND", new TextLineAttributes
        {
            Color = Color.Blue,
            Border = true,
            Width = 0,
            Height = 0
        });

        for (int i = 0; i < star.NearbyCities?.Locations.Count; i++)
        {
            string locationName = star.NearbyCities.GetLocationName(i);
            nearbyObs.AddLine($"{locationName,-14}    No Report");
        }

        _dataTransmitter.AddFrame(nearbyObs.Build());
        Console.WriteLine($"[DataDownloader] Page {(int)Page.LatestObservations} sent");
    }

    /// <summary>
    /// Send "No Report" pages for primary forecast location
    /// </summary>
    /// <param name="star"></param>
    private void SendForecastNoReportPages(Config.WeatherStar star)
    {
        TextLineAttributes smallHeight = new()
        {
            Color = Color.Blue,
            Border = true,
            Width = 0,
            Height = 0
        };

        // Forecast narrative pages
        for (int i = 0; i < 3; i++)
        {
            // Build page
            int actualPageNum = (int)Page.Forecast1 + i;
            PageBuilder pageBuilder = new PageBuilder(actualPageNum, Address.FromSwitches(star.Switches), _omcw)
                .AddLine("        Your TWC Forecast", smallHeight)
                .AddLine("")
                .AddLine("No Report");

            _dataTransmitter.AddFrame(pageBuilder.Build());
            Console.WriteLine($"[DataDownloader] Page {actualPageNum} sent");
        }

        // Extended forecast
        PageBuilder extForecastPage = new PageBuilder((int)Page.ExtendedForecast, Address.FromSwitches(star.Switches), _omcw)
            .AddLine("       Extended Forecast")
            .AddLine("")
            .AddLine("No Report");

        _dataTransmitter.AddFrame(extForecastPage.Build());
        Console.WriteLine($"[DataDownloader] Page {(int)Page.ExtendedForecast} sent");

        // Regional forecast
        PageBuilder regionalFcst = new PageBuilder((int)Page.RegionalForecast, Address.FromSwitches(star.Switches), _omcw)
            .AddLine("   Forecast Across The Region")
            .AddLine("City           Weather   Low  Hi", smallHeight);

        for (int i = 0; i < star.RegionalCities?.Locations.Count; i++)
        {
            if (i > 6) break; // Only 7 cities are allowed
            string locationName = star.RegionalCities.GetLocationName(i);
            regionalFcst.AddLine($"{locationName,-14} No Report");
        }

        _dataTransmitter.AddFrame(regionalFcst.Build());
        Console.WriteLine($"[DataDownloader] Page {(int)Page.RegionalForecast} sent");
    }

    private async Task UpdateAlmanac()
    {
        foreach (Config.WeatherStar star in _config.Stars)
        {
            Console.WriteLine($"[DataDownloader] Updating almanac for {star.LocationName}");

            // Get local time for this star's location
            DateTime localStarTime = TimeZoneInfo.ConvertTime(DateTime.Now, star.GetTimeZoneInfo());
            int currentDay = localStarTime.Day;
            int currentMonth = localStarTime.Month;

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
            AlmanacResponse? dailyAlmanacData = JsonSerializer.Deserialize<AlmanacResponse>(responseBody);

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
            AlmanacResponse? monthlyAlmanacData = JsonSerializer.Deserialize<AlmanacResponse>(responseBody2);

            if (monthlyAlmanacData == null)
            {
                Console.WriteLine($"[DataDownloader] Failed to download monthly almanac for {star.LocationName}");
                continue;
            }

            // Build padding into strings
            string todayDayName = localStarTime.Date.ToString("dddd");
            string tomorrowDayName = localStarTime.Date.AddDays(1).ToString("dddd");
            string monthName = localStarTime.Date.ToString("MMMM");
            // When it's Tuesday, Wednesday needs to be moved over 1 character left
            int leftDayPadding = localStarTime.Date.DayOfWeek == DayOfWeek.Tuesday ? 9 : 10;

            string precipLine = "";
            if (monthlyAlmanacData.PrecipitationAverage[0] != null)
            {
                precipLine = $"Normal {monthName} Precip".PadRight(24, ' ') +
                             $"{monthlyAlmanacData.PrecipitationAverage[0]}in".PadLeft(7, ' ');
            }

            // Get sunset and sunrise
            Coordinate cToday = new(star.GetLat(), star.GetLon(), localStarTime);
            Coordinate cTomorrow = new(star.GetLat(), star.GetLon(), localStarTime.AddDays(1));

            TextLineAttributes smallHeight = new()
            {
                Color = Color.Blue,
                Border = true,
                Width = 0,
                Height = 0
            };

            // We also need to convert these values to the star's time zone 
            DateTime todaySunRise = TimeZoneInfo.ConvertTime(cToday.CelestialInfo.SunRise ?? DateTime.MinValue, star.GetTimeZoneInfo()).ToLocalTime();
            DateTime todaySunSet = TimeZoneInfo.ConvertTime(cToday.CelestialInfo.SunSet ?? DateTime.MinValue, star.GetTimeZoneInfo()).ToLocalTime();
            DateTime tomorrowSunRise = TimeZoneInfo.ConvertTime(cTomorrow.CelestialInfo.SunRise ?? DateTime.MinValue, star.GetTimeZoneInfo()).ToLocalTime();
            DateTime tomorrowSunSet = TimeZoneInfo.ConvertTime(cTomorrow.CelestialInfo.SunSet ?? DateTime.MinValue, star.GetTimeZoneInfo()).ToLocalTime();

            // Build page
            DataFrame[] almanacPage = new PageBuilder((int)Page.Almanac, Address.FromSwitches(star.Switches), _omcw)
                .AddLine("  The Weather Channel Almanac   ")
                .AddLine("", smallHeight)
                .AddLine($"              {todayDayName.PadRight(leftDayPadding)}{tomorrowDayName}")
                .AddLine("Sunrise      " + todaySunRise.ToString("h:mm tt").PadLeft(8) + "  " + tomorrowSunRise.ToString("h:mm tt").PadLeft(8))
                .AddLine("Sunset       " + todaySunSet.ToString("h:mm tt").PadLeft(8) + "  " + tomorrowSunSet.ToString("h:mm tt").PadLeft(8))
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
        TextLineAttributes smallHeight = new()
        {
            Color = Color.Blue,
            Border = true,
            Width = 0,
            Height = 0
        };

        foreach (Config.WeatherStar star in _config.Stars)
        {
            Console.WriteLine($"[DataDownloader] Updating forecast for {star.LocationName}");

            // Build list of locations to get current forecast for
            List<string> locations = [];

            // Add primary location
            locations.Add(star.Location);

            // Add regional cities
            locations
                .AddRange(star.RegionalCities?.Geocodes.Where(str => !string.IsNullOrEmpty(str)) ?? []);

            // Make HTTP request
            string locationsString = string.Join(';', locations);

            // Make HTTP request
            HttpResponseMessage httpResponseMessage =
                await Util.HttpClient.GetAsync($"https://api.weather.com/v3/aggcommon/v3-wx-forecast-daily-5day?geocodes={locationsString}&language=en-US&format=json&units=e&apiKey={_config.APIKey}");

            // Make sure the request was successful
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                Console.WriteLine($"[DataDownloader] Failed to download forecast for {star.LocationName}: HTTP " + (int)httpResponseMessage.StatusCode);
                SendForecastNoReportPages(star);
                continue;
            }

            // Parse response as JSON
            string responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
            List<AggregateResponse>? forecastDatas = JsonSerializer.Deserialize<List<AggregateResponse>?>(responseBody);

            if (forecastDatas == null)
            {
                Console.WriteLine($"[DataDownloader] Failed to download forecast data for {star.LocationName}");
                SendForecastNoReportPages(star);
                continue;
            }

            // //
            // Daypart forecast for primary location
            // //

            // Get primary location from API data
            ForecastResponse? forecastData = forecastDatas.FirstOrDefault(item => item.Id == star.Location)?.DailyForecast5DayResponse;

            if (forecastData == null)
            {
                Console.WriteLine($"[DataDownloader] Failed to download forecast for {star.LocationName}");
                SendForecastNoReportPages(star);
            }
            else
            {
                // List to hold lines that overflow onto the next page
                List<string> overflowLines = [];

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
                    List<string> narrativeLines = [];

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
                            narrativeLines.Add("*" + Util.CenterString(alert.EventDescription ?? "Unknown Event", 30) + "*");
                            if (alert.EndTimeUTC != null)
                            {
                                DateTime endTime = DateTimeOffset.FromUnixTimeSeconds(alert.EndTimeUTC ?? 0).LocalDateTime;
                                narrativeLines.Add("*" + Util.CenterString("Until " + endTime.ToString("htt ddd"), 30) + "*");
                            }
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

                // //
                // Extended forecast for primary location
                // //

                // Get day names
                string day1Name = forecastData.DayOfWeek[2]; // Start at index 2
                string day2Name = forecastData.DayOfWeek[3];
                string day3Name = forecastData.DayOfWeek[4];

                // Get daypart conditions, we use the day part of the daypart, so we skip the odd indexes
                // Split the text up for word wrapping too
                List<string> day1Cond = Util.WordWrap(forecastData.Daypart[0].GetFormattedWxPhrase(4), 10);
                List<string> day2Cond = Util.WordWrap(forecastData.Daypart[0].GetFormattedWxPhrase(6), 10);
                List<string> day3Cond = Util.WordWrap(forecastData.Daypart[0].GetFormattedWxPhrase(8), 10);

                // Get defaults for each line of the conditions
                string day1CondLine1 = day1Cond.ElementAtOrDefault(0) ?? "";
                string day1CondLine2 = day1Cond.ElementAtOrDefault(1) ?? "";
                string day2CondLine1 = day2Cond.ElementAtOrDefault(0) ?? "";
                string day2CondLine2 = day2Cond.ElementAtOrDefault(1) ?? "";
                string day3CondLine1 = day3Cond.ElementAtOrDefault(0) ?? "";
                string day3CondLine2 = day3Cond.ElementAtOrDefault(1) ?? "";

                string day1Hi = forecastData.TemperatureMax[2].ToString() ?? ""; // Start at index 2
                string day1Lo = forecastData.TemperatureMin[2].ToString() ?? "";
                string day2Hi = forecastData.TemperatureMax[3].ToString() ?? "";
                string day2Lo = forecastData.TemperatureMin[3].ToString() ?? "";
                string day3Hi = forecastData.TemperatureMax[4].ToString() ?? "";
                string day3Lo = forecastData.TemperatureMin[4].ToString() ?? "";

                PageBuilder extForecastPage = new PageBuilder((int)Page.ExtendedForecast, Address.FromSwitches(star.Switches), _omcw)
                    .AddLine("       Extended Forecast")
                    .AddLine("")
                    .AddLine($" {day1Name,-11}{day2Name,-11}{day3Name,-11}")
                    .AddLine($" {day1CondLine1,-11}{day2CondLine1,-11}{day3CondLine1,-11}")
                    .AddLine($" {day1CondLine2,-11}{day2CondLine2,-11}{day3CondLine2,-11}")
                    .AddLine("")
                    .AddLine($" Lo: {day1Lo,-3}    Lo: {day2Lo,-3}    Lo: {day3Lo,-3}")
                    .AddLine($" Hi: {day1Hi,-3}    Hi: {day2Hi,-3}    Hi: {day3Hi,-3}");

                _dataTransmitter.AddFrame(extForecastPage.Build());
                Console.WriteLine($"[DataDownloader] Page {(int)Page.ExtendedForecast} sent");
            } // end primary forecastData null check

            // // 
            // Regional Forecast
            // //

            PageBuilder regionalFcst = new PageBuilder((int)Page.RegionalForecast, Address.FromSwitches(star.Switches), _omcw);
            regionalFcst.AddLine("   Forecast Across The Region");
            regionalFcst.AddLine("City           Weather   Low  Hi", smallHeight);

            for (int i = 0; i < star.RegionalCities?.Locations.Count; i++)
            {
                if (i > 6) break; // Only 7 cities are allowed
                string locationName = star.RegionalCities.GetLocationName(i);
                string geocode = star.RegionalCities.Geocodes.ElementAtOrDefault(i) ?? "";
                forecastData = forecastDatas.FirstOrDefault(item => item.Id == geocode)?.DailyForecast5DayResponse;

                if (forecastData == null)
                {
                    regionalFcst.AddLine($"{locationName,-14} No Report");
                }
                else
                {
                    // We use the 2 index on everything here so we get tomorrow's forecast (0 is today, 1 is tonight, 2 is tomorrow)
                    string cond = (forecastData.Daypart[0].WxPhraseShort[2] ?? "").PadRight(10);
                    // Use index 1 here to get tomorrow's hi and low temp (0 = today, 1 = tomorrow)
                    string lowTemp = (forecastData.TemperatureMin[1].ToString() ?? "").PadRight(3);
                    string hiTemp = (forecastData.TemperatureMax[1].ToString() ?? "").PadRight(3);
                    regionalFcst.AddLine($"{locationName,-14} {cond} {lowTemp} {hiTemp}");
                }
            }

            _dataTransmitter.AddFrame(regionalFcst.Build());
            Console.WriteLine($"[DataDownloader] Page {(int)Page.RegionalForecast} sent");
        }
    }
}