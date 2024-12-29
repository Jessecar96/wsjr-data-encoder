using JrEncoderLib.DataTransmitter;
using JrEncoderLib.Frames;
using JrEncoderLib.StarAttributes;

namespace JrEncoder;

class Program
{
    static void Main(string[] args)
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

        // Build our OMCW
        OMCW omcw = OMCW.Create()
            .BottomSolid()
            .TopSolid()
            .TopPage(50)
            .RegionSeparator()
            .LDL(LDLStyle.DateTime)
            .Commit();

        // Init data transmitter, sets up DDS module
        GPIODataTransmitter transmitter = new(omcw);
        transmitter.Init();

        // Background thread for data transmission
        Task.Run(() => transmitter.Run());

        // Init data downloader
        DataDownloader downloader = new(config, transmitter, omcw);

        // Background thread for data downloading
        Task.Run(async () =>
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

        // Send date & time
        TimeOfDayFrame todFrame = TimeOfDayFrame.Now(omcw, 0b111);
        transmitter.AddFrame(todFrame);

        omcw
            .BottomSolid()
            .TopSolid()
            .TopPage(1)
            .RegionSeparator()
            .LDL(LDLStyle.DateTime)
            .Commit();

        while (true)
        {
            Thread.Sleep(1000);
        }
    }
}