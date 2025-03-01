using JrEncoderLib.DataTransmitter;
using JrEncoderLib.Frames;
using JrEncoderLib.StarAttributes;

namespace JrEncoder;

public class TimeUpdater(Config config, DataTransmitter dataTransmitter, OMCW omcw)
{
    private Config _config = config;
    private DataTransmitter _dataTransmitter = dataTransmitter;
    private OMCW _omcw = omcw;
    
    public void Run()
    {
        Console.WriteLine("[TimeUpdater] Running...");
        UpdateTime();
        _ = Task.Run(async () =>
        {
            // Update time every 2 minutes
            using PeriodicTimer timer = new(TimeSpan.FromMinutes(2));
            while (await timer.WaitForNextTickAsync())
                UpdateTime();
        });
    }

    private void UpdateTime()
    {
        Console.WriteLine("[TimeUpdater] Sending time update");
        
        // Look if any stars have all 0 switches, or config is set to force clock set
        if (_config.ForceClockSet || _config.Stars.Any(star => star.Switches == "00000000"))
        {
            // Set all time zones to the zone of the first star defined
            Config.WeatherStar star = _config.Stars[0];
            Console.WriteLine($"[TimeUpdater] Using global time zone {star.GetTimeZoneIdentifier()}");

            // There are 8 possible time zones 0-7
            for (int i = 0; i <= 7; i++)
            {
                TimeOfDayFrame todFrame = TimeOfDayFrame.Now(_omcw, i, star.GetTimeZoneInfo());
                _dataTransmitter.AddFrame(todFrame);
            }
        }
        else
        {
            // Set the time zone of each star individually
            List<int> usedTimeZones = [];
            foreach (Config.WeatherStar star in _config.Stars)
            {
                int timezone = Address.GetTimeZone(star.Switches);
                Console.WriteLine($"[TimeUpdater] Star {star.LocationName} is using time zone {star.GetTimeZoneIdentifier()} with address {timezone}");

                // Check if this time zone address was already used, if so don't send it again
                if (usedTimeZones.Contains(timezone))
                {
                    Console.WriteLine($"[TimeUpdater] ERROR: Star {star.LocationName} has the same time zone address as another. Time will not be set.");
                    continue;
                }

                // Send the time for this zone
                TimeOfDayFrame todFrame = TimeOfDayFrame.Now(_omcw, timezone, star.GetTimeZoneInfo());
                _dataTransmitter.AddFrame(todFrame);

                // Add this to our list of used time zones
                usedTimeZones.Add(timezone);
            }
        }
    }
}