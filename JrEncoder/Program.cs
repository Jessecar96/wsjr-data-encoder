using JrEncoder.StarAttributes;

namespace JrEncoder;

class Program
{
    static void Main(string[] args)
    {
        DateTime currentTime = DateTime.Now;
        int hour = int.Parse(currentTime.ToString("%h"));
        int amPm = currentTime.Hour > 12 ? 1 : 0;
        
        // First OMCW, show only LDL
        OMCW omcw = new(false, false, false, false, false, true, true, true, 10, 3);
        TimeOfDayFrame todFrame = new(omcw, 8, 1, currentTime.Month, currentTime.Day, hour, currentTime.Minute, currentTime.Second, amPm);

        DataFrame[] testPage = new PageBuilder(10, new Address(1, 2, 3, 4), omcw)
            .AddLine("Test Line 1")
            .AddLine("Test Line 2")
            .AddLine("Test Line 3")
            .Build();

        DataFrame[] ldlPage = new PageBuilder(50, new Address(1, 2, 3, 4), omcw)
            .AddLine("This should show on the LDL :)")
            .AddLine("This should also work too")
            .AddLine("LDL line 3")
            .Build();

        DataTransmitter transmitter = new();
        transmitter.Init(todFrame, ldlPage, testPage);
    }
}