using System.Xml.Serialization;
using System.Linq;
using JrEncoderLib;
using JrEncoderLib.DataTransmitter;
using JrEncoderLib.Frames;
using JrEncoderLib.StarAttributes;

namespace JrEncoder;

class Program
{
    private static OMCW omcw;
    private static GPIODataTransmitter transmitter;

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
        omcw = OMCW.Create()
            .BottomSolid(false)
            .TopSolid(false)
            .TopPage(0)
            .RegionSeparator(false)
            .LDL(LDLStyle.DateTime)
            .Commit();

        // Init data transmitter, sets up DDS module
        transmitter = new(omcw);
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

        // Load flavor config
        string flavorsFilePath = Path.Combine(Util.GetExeLocation(), "Flavors.xml");
        Flavors? flavors = null;
        if (File.Exists(flavorsFilePath))
        {
            XmlSerializer serializer = new(typeof(Flavors));
            using StreamReader reader = new(flavorsFilePath);
            flavors = (Flavors?)serializer.Deserialize(reader);
        }

        if (flavors == null)
        {
            // No flavors :((
            Console.WriteLine("ERROR: Failed to load Flavors.xml");
            ShowErrorMessage("Failed to load Flavors.xml");
            return;
        }

        // Check if looping is configured
        if (config.LoopFlavor != null)
        {
            // Find the flavor defined in Flavors.xml
            Flavor? flavor = flavors.Flavor.FirstOrDefault(el => el.Name == config.LoopFlavor);

            // Could not find that flavor
            if (flavor == null)
            {
                Console.WriteLine($"ERROR: Flavor \"{config.LoopFlavor}\" does not exist in Flavors.xml");
                ShowErrorMessage($"Flavor \"{config.LoopFlavor}\" does not exist in Flavors.xml");
                return;
            }

            // Start looping that flavor
            while (true)
            {
                foreach (FlavorPage page in flavor.Page)
                {
                    // Make sure that page exists
                    if (!Enum.TryParse(page.Name, out Page newPage))
                    {
                        Console.WriteLine($"ERROR: Invalid page \"{page.Name}\"");
                        ShowErrorMessage($"Invalid page \"{page.Name}\"");
                        return;
                    }
                    
                    // Check if this page is changing LDL style
                    if (!string.IsNullOrEmpty(page.LDL))
                    {
                        // Make sure that LDL style exists
                        if (!Enum.TryParse(page.LDL, out LDLStyle ldlStyle))
                        {
                            ShowErrorMessage($"Invalid LDL Style \"{page.LDL}\"");
                            return;
                        }
                        // Set that LDL style
                        omcw.LDL(ldlStyle);
                    }

                    // Switch to that page
                    omcw
                        .TopSolid(true)
                        .TopPage((int)newPage);

                    omcw.Commit();
                    
                    Console.WriteLine("Switched to page " + newPage);
                    
                    // Wait for its duration
                    Thread.Sleep(page.Duration * 1000);
                }
            }
        } // end looping check
        
    }

    /// <summary>
    /// Show an error message page forever
    /// Use only when a fatal error has occurred
    /// </summary>
    /// <param name="message"></param>
    private static void ShowErrorMessage(string message)
    {
        PageBuilder page = new PageBuilder((int)Page.Error, Address.All, omcw)
            .AddLine(Util.CenterString("ATTENTION CABLE OPERATOR"), new TextLineAttributes { Color = Color.Diarrhea })
            .AddLine(Util.CenterString("An Error Has Occurred"), new TextLineAttributes { Color = Color.Diarrhea })
            .AddLine(Util.CenterString("Restart Software Once Corrected"), new TextLineAttributes { Color = Color.Diarrhea })
            .AddLine("", new TextLineAttributes { Color = Color.Diarrhea, Height = 0});

        // Word wrap message and add it
        List<string> messages = Util.WordWrap(message, 32);
        foreach (string s in messages)
            page.AddLine(Util.CenterString(s), new TextLineAttributes { Color = Color.Diarrhea });
        
        transmitter.AddFrame(page.Build());
        
        Thread.Sleep(500);
        omcw.TopSolid(true).TopPage(0).Commit();
        Thread.Sleep(500);

        // Loop forever
        while (true)
        {
            omcw.TopSolid(true).TopPage((int)Page.Error).Commit();
            Thread.Sleep(10000);
        }
    }
}