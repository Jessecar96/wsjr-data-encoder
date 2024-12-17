using System.Text;

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
        
        DataFrame[] testPage = new PageBuilder(10)
            .setOMCW(omcw)
            .setPageAttributes(new PageAttributes(false, false, false, false, false, false))
            .setLineAttributes(
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue)
            )
            .addLine(1, "Test Line 1")
            .addLine(2, "Test Line 2")
            .addLine(3, "Test Line 3")

            .setAddress(new Address(1, 2, 3, 4))
            .build();
        
        DataFrame[] ldlPage = new PageBuilder(50)
            .setOMCW(omcw)
            .setPageAttributes(new PageAttributes(false, false, false, false, false, false))
            .setLineAttributes(
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue)
            )
            .addLine(1, "This should show on the LDL :)")
            .addLine(2, "This should also work too")
            .addLine(3, "LDL line 3")
            .setAddress(new Address(1, 2, 3, 4))
            .build();
        
        /*
        DataTransmitter transmitter = new();
        transmitter.Init(todFrame, ldlPage, testPage);
        */
    }
}