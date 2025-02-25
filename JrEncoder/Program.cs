using JrEncoderLib;
using JrEncoderLib.DataTransmitter;
using JrEncoderLib.Frames;
using JrEncoderLib.StarAttributes;

namespace JrEncoder;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting WeatherSTAR Data Transmitter");
        Console.WriteLine("Written by Jesse Cardone, 2024");

        // Config creation parameter
        if (args.Contains("--create-config"))
        {
            if (File.Exists(Config.GetPath()))
            {
                Console.WriteLine("config.json already exists. Please delete it to create a new one.");
                return;
            }
            
            Config.CreateConfig();
            Console.WriteLine("Created config.json, program will now exit.");
            return;
        }

        // Load config
        Config config;
        try
        {
            config = Config.LoadConfig();
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to load config: " + e.Message);
            return;
        }

        // Build our OMCW of defaults
        OMCW omcw = OMCW.Create()
            .BottomSolid(false)
            .TopSolid(false)
            .TopPage(0)
            .RegionSeparator(false)
            .LDL(LDLStyle.DateTime)
            .Commit();

        // Init data transmitter, sets up DDS module
        GPIODataTransmitter transmitter = new(omcw);
        transmitter.Init();

        // Background thread for data transmission
        _ = Task.Run(() => transmitter.Run());

        // Init data downloader
        DataDownloader downloader = new(config, transmitter, omcw);

        // Send date & time
        if (config.ForceClockSet)
        {
            // Set all time zones to the zone of the first star defined
            Config.WeatherStar star = config.Stars[0];
            Console.WriteLine($"Using time zone {star.GetTimeZoneIdentifier()}");
            
            // There are 8 possible time zones 0-7
            for (int i = 0; i < 7; i++)
            {
                TimeOfDayFrame todFrame = TimeOfDayFrame.Now(omcw, i, star.GetTimeZoneInfo());
                transmitter.AddFrame(todFrame);
            }
        }
        else
        {
            // Set the time zone of each star individually
            List<int> usedTimeZones = [];
            foreach (Config.WeatherStar star in config.Stars)
            {
                int timezone = Address.GetTimeZone(star.Switches);
                Console.WriteLine($"Star {star.LocationName} is using time zone {star.GetTimeZoneIdentifier()} with address {timezone}");

                // Check if this time zone address was already used, if so don't send it again
                if (usedTimeZones.Contains(timezone))
                {
                    Console.WriteLine($"ERROR: Star {star.LocationName} has the same time zone address as another. Time will not be set.");
                    continue;
                }
                
                // Send the time for this zone
                TimeOfDayFrame todFrame = TimeOfDayFrame.Now(omcw, timezone, star.GetTimeZoneInfo());
                transmitter.AddFrame(todFrame);
                
                // Add this to our list of used time zones
                usedTimeZones.Add(timezone);
            }
        }

        // Updating page
        DataFrame[] updatePage = new PageBuilder(41, Address.All, omcw)
            .AddLine("                                ")
            .AddLine("                                ")
            .AddLine("                                ")
            .AddLine("           Please Wait          ")
            .AddLine("  Information is being updated  ")
            .Build();
        transmitter.AddFrame(updatePage);

        // Show updating page
        omcw
            .BottomSolid()
            .TopSolid()
            .TopPage(41)
            .RegionSeparator()
            .LDL(LDLStyle.DateTime)
            .Commit();

        // Update all records
        await downloader.UpdateAll();

        // Background thread for data downloading
        try
        {
            downloader.Run();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        await Task.Delay(500);

        // Start looping pages
        while (true)
        {
            for (int i = 1; i <= 7; i++)
            {
                omcw.TopPage((int)Page.CurrentConditions).Commit();
                Thread.Sleep(config.PageInterval * 1000);
                omcw.TopPage((int)Page.Almanac).Commit();
                Thread.Sleep(config.PageInterval * 1000);
                omcw.TopPage((int)Page.Forecast1).LDL(LDLStyle.LocalCrawl).Commit();
                Thread.Sleep(config.PageInterval * 1000);
                omcw.TopPage((int)Page.Forecast2).Commit();
                Thread.Sleep(config.PageInterval * 1000);
                omcw.TopPage((int)Page.Forecast3).Commit();
                Thread.Sleep(config.PageInterval * 1000);
                omcw.TopPage((int)Page.ExtendedForecast).LDL(LDLStyle.DateTime).Commit();
                Thread.Sleep(config.PageInterval * 1000);
                omcw.TopPage((int)Page.LatestObservations).Commit();
                Thread.Sleep(config.PageInterval * 1000);
            }
        }
    }
}