namespace JrEncoder;

public enum Page
{
    CurrentConditions = 1, // Current conditions for the primary location
    Almanac = 2,
    Forecast1 = 3, // 36 hour forecast, page 1
    Forecast2 = 4, // 36 hour forecast, page 2
    Forecast3 = 5, // 36 hour forecast, page 3
    ExtendedForecast = 6,
    LatestObservations = 7, // The current weather, temperature, and winds in seven nearby cities
    RegionalObservations = 8,
    RegionalForecast = 9,
    TravelCitiesForecast1 = 10,
    TravelCitiesForecast2 = 11,
    TravelCitiesForecast3 = 12,
    WxWarning = 20, // Used for weather warnings, these can be many pages long
    Error = 49, // Used for error messages
    LDL = 50, // LDL page, shows current conditions
}