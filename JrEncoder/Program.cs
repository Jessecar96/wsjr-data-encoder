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

        // Load config
        Config config;
        try
        {
            config = Config.LoadConfig();
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to load config");
            Console.WriteLine(e);
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
        Task.Run(() => transmitter.Run());

        // Init data downloader
        DataDownloader downloader = new(config, transmitter, omcw);

        // Send date & time
        TimeOfDayFrame todFrame = TimeOfDayFrame.Now(omcw, 0b111);
        transmitter.AddFrame(todFrame);

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
        _ = Task.Run(async () =>
        {
            try
            {
                await downloader.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        });

        // Start looping pages
        while (true)
        {
            for (int i = 1; i <= 5; i++)
            {
                omcw.TopPage(i).Commit();
                Thread.Sleep(config.PageInterval * 1000);
            }
        }
    }
}