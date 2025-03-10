# The Weather Star Data Encoder
This is the data provider for [The Weather STAR Revamped](https://github.com/Jessecar96/wsjr-modulator) modulator hardware. This project is still a work in progress.

This project is distributed under the GNU General Public License v3.0.

## Installation
See [INSTALL.md](INSTALL.md)

## Configuration
The first run of the install script will generate a config.json file in `$HOME/jrencoder`. Below I've provided a sample config with details on what each setting does

```js
{
  "apikey": "xxxxx", // API key for the weather.com API
  "force_clock_set": false, // Setting this to true will force all stars to use the same time zone
  "loop_flavor": "L", // Flavor (definded in Flavors.xml) that all stars will run continuously
  "stars": [
    {
      "switches": "00EF08EF", // "Switches" for this star. See INSTALL.md on how to get these from your star
      "location": "30.58,-96.36", // Primary location coordinates
      "location_name": "College Station", // Primary location name
      "nearby_cities": { // Used for "Latest Observations" page
        "location_name": [
          "Bryan / C.S.", // Name that will show on the LF
          "Houston",
          // ...up to 7 cities
        ],
        "geocode": [
          "30.67,-96.36", // Equivalent coordinates for each "location_name"
          "29.75,-95.37",
        ]
      },
      "regional_cities": { // Cities used for "Regional Observations" and "Regional Forecast" pages
        "location_name": [
          "San Angelo, TX",
          "Corpus Christi, TX",
          // ...up to 7 cities
        ],
        "geocode": [
          "31.45,-100.45",
          "27.80,-97.40",
        ]
      }
    }
  ]
}
```

## Working
- Setting the clock
- Multiple time zones
- Addressing multiple weather stars
- NWS Headlines
- Severe warnings/advisories
- Current conditions
- Almanac
- Text-based 36 hour forecast
- Extended forecast
- Latest observations
- Regional observations
- Regional forecast
- Travel cities forecast

## TODO
- Tides
- Air quality
