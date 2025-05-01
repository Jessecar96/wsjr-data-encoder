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
    private static Flavors? flavors;
    private static bool flavorRunning;
    private static Config config;
    private static TimeUpdater timeUpdater;
    private static GPIODataTransmitter transmitter;
    public static DataDownloader downloader;

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

        // Setup http client
        Util.HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("JrEncoder/1.0");

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

        // Load config.json
        LoadConfig("config.json");

        // Init time updater
        timeUpdater = new(config, transmitter, omcw);
        timeUpdater.Run();

        // Init data downloader
        downloader = new(config, transmitter, omcw);

        // Start MQTT server
        MQTTServer server = new();
        _ = Task.Run(() => server.Run());

        // MQTT Client
        MQTTClient client = new(omcw);
        _ = Task.Run(() => client.Run());

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
            ShowErrorMessage("Download failed: " + e.Message, true);
            Console.WriteLine(e);
        }

        await Task.Delay(500);

        // Load flavor config
        string flavorsFilePath = Path.Combine(Util.GetExeLocation(), "Flavors.xml");
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
            ShowErrorMessage("Failed to load Flavors.xml", true);
            return;
        }

        // Check if looping is configured
        if (config.LoopFlavor != null)
        {
            // Loop that flavor forever
            while (true)
                RunFlavor(config.LoopFlavor);
        }

        // Default no loop state
        omcw.TopSolid(false)
            .TopPage(0)
            .BottomSolid(false)
            .RegionSeparator(false)
            .LDL(LDLStyle.DateTime)
            .Commit();

        Console.WriteLine("Ready for commands! Looping is not enabled.");

        // Forever...
        while (true)
            Thread.Sleep(1000);
    }

    /// <summary>
    /// Load a config file
    /// </summary>
    public static void LoadConfig(string fileName)
    {
        try
        {
            config = Config.LoadConfig(fileName);
            if (downloader != null)
                downloader.SetConfig(config);
            if (timeUpdater != null)
                timeUpdater.SetConfig(config);
            Console.WriteLine("Loaded config file " + fileName);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to load {fileName}: " + e.Message);
            ShowErrorMessage($"Failed to load {fileName}: " + e.Message, true);
        }
    }

    public static void RunFlavor(string flavorName)
    {
        if (flavorRunning)
        {
            Console.WriteLine("ERROR: Flavor is already running. Not running another!");
            return;
        }

        // Make sure flavors are defined
        if (flavors == null)
        {
            Console.WriteLine("ERROR: Flavors were not loaded");
            ShowErrorMessage("Flavors were not loaded");
            return;
        }

        // Find the flavor defined in Flavors.xml
        Flavor? flavor = flavors.Flavor.FirstOrDefault(el => el.Name == flavorName);

        // Could not find that flavor
        if (flavor == null)
        {
            Console.WriteLine($"ERROR: Flavor \"{flavorName}\" does not exist in Flavors.xml");
            ShowErrorMessage($"Flavor \"{flavorName}\" does not exist in Flavors.xml");
            return;
        }

        Console.WriteLine($"Running flavor \"{flavor.Name}\"");
        flavorRunning = true;

        // TODO: Save OMCW state to restore to after

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
                .BottomSolid(true)
                .RegionSeparator(true)
                .TopPage((int)newPage);

            omcw.Commit();

            Console.WriteLine("Switched to page " + newPage);

            // Wait for its duration
            Thread.Sleep(page.Duration * 1000);
        }

        // we done!
        flavorRunning = false;
        Console.WriteLine($"Flavor \"{flavor.Name}\" complete");

        // Default no loop state
        omcw.TopSolid(false)
            .TopPage(0)
            .BottomSolid(false)
            .RegionSeparator(false)
            .LDL(LDLStyle.DateTime)
            .Commit();
    }

    public static void ShowWxWarning(string message, WarningType type, Address address, OMCW omcw)
    {
        ShowWxWarning(message.Split('\n').ToList(), type, address, omcw);
    }

    public static void ShowWxWarning(List<string> message, WarningType type, Address address, OMCW omcw)
    {
        // Split that into chunks of 9 lines each
        string[][] chunks = message.Chunk(9).ToArray();

        int pageOffset = 0;
        for (int i = 0; i < chunks.Length; i++)
        {
            // Make a new page
            PageBuilder page = new PageBuilder((int)Page.WxWarning + pageOffset, address, omcw);

            // Set page attributes
            bool firstPage = i == 0;
            bool lastPage = i == chunks.Length - 1;
            bool multiPage = chunks.Length != 1;
            PageAttributes pageAttributes = new PageAttributes
            {
                Roll = true, // Every page needs to roll
                Chain = (multiPage && !lastPage) // Set chain when we have more than 1 page, and it's not the last page
            };

            // Set warning/advistory depending on type
            if (type == WarningType.Warning)
                pageAttributes.Warning = firstPage; // Only the first page in the chain gets the warning bit set
            else if (type == WarningType.Advisory)
                pageAttributes.Advisory = firstPage; // Only the first page in the chain gets the warning bit set

            page.Attributes(pageAttributes);

            // Add all the chunked lines
            foreach (string line in chunks[i])
                page.AddLine(line, new TextLineAttributes
                {
                    Color = (type == WarningType.Warning) ? Color.Red : Color.Brown
                });

            // Add to offset for the next loop
            pageOffset++;

            // Send it
            transmitter.AddFrame(page.Build());
            Console.WriteLine("Sent page " + page.PageNumber);
        }

        Console.WriteLine("Weather warning activated");
    }

    /// <summary>
    /// Show an error message page
    /// </summary>
    /// <param name="message"></param>
    /// <param name="fatal">If this message should show forever, locking up the program</param>
    public static void ShowErrorMessage(string message, bool fatal = false)
    {
        PageBuilder page = new PageBuilder((int)Page.Error, Address.All, omcw)
            .AddLine(Util.CenterString("ATTENTION CABLE OPERATOR"), new TextLineAttributes { Color = Color.Diarrhea })
            .AddLine(Util.CenterString("An Error Has Occurred"), new TextLineAttributes { Color = Color.Diarrhea })
            .AddLine(Util.CenterString("Restart Software Once Corrected"), new TextLineAttributes { Color = Color.Diarrhea })
            .AddLine("", new TextLineAttributes { Color = Color.Diarrhea, Height = 0 });

        // Word wrap message and add it
        List<string> messages = Util.WordWrap(message, 32);
        foreach (string s in messages)
            page.AddLine(Util.CenterString(s), new TextLineAttributes { Color = Color.Diarrhea });

        transmitter.AddFrame(page.Build());

        // Wait for the page to load into memory...
        Thread.Sleep(500);

        // Show blank page
        omcw.TopSolid(true)
            .BottomSolid(true)
            .RegionSeparator(true)
            .TopPage(0)
            .Commit();

        // Wait for that to happen...
        Thread.Sleep(500);

        // Show error page
        omcw.TopSolid(true)
            .BottomSolid(true)
            .RegionSeparator(true)
            .TopPage((int)Page.Error)
            .Commit();

        // Fatal error will cause the program to stop here forever
        if (fatal)
            while (true)
                Thread.Sleep(1000);
    }
}