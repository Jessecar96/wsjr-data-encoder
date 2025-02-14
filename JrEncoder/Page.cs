namespace JrEncoder;

public enum Page
{
    CurrentConditions = 1, // Current conditions for the primary location
    Almanac = 2,
    Forecast1 = 3, // 36 hour forecast, page 1
    Forecast2 = 4, // 36 hour forecast, page 2
    Forecast3 = 5, // 36 hour forecast, page 3
    LDL = 50, // LDL page, shows current conditions
}