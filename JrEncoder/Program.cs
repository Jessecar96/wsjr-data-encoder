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

        // Send date & time to all configured stars
        // TODO: Support multiple timezones
        foreach (Config.WeatherStar star in config.Stars)
        {
            Console.WriteLine("Sending time for timezone: " + Address.GetTimeZone(star.Switches));
            TimeOfDayFrame todFrame = TimeOfDayFrame.Now(omcw, Address.GetTimeZone(star.Switches));
            transmitter.AddFrame(todFrame);
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
                omcw.TopPage(i).Commit();
                Thread.Sleep(config.PageInterval * 1000);
            }
        }
    }
}