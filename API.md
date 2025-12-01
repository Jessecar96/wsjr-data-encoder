# API Specificiation

## GET /config/get
Returns current loaded config

## POST /config/set
Sets current active config

## POST /presentation/run
Runs a presenation flavor

| Parameter | Description       | Example    |
|-----------|-------------------|------------|
| flavor    | Flavor name       | L          |
| time      | Start time (unix) | 1764559358 |

## POST /presentation/loop
Runs a presenation flavor until cancel is called

| Parameter | Description       | Example    |
|-----------|-------------------|------------|
| flavor    | Flavor name       | L          |

## POST /presentation/cancel
Cancels any currently running presentation

## POST /alert/send
Send a scrolling weather alert

| Parameter | Description       | Example             |
|-----------|-------------------|---------------------|
| text      | Text of the alert | Hello World         |
| type      | Type of alert     | Warning or Advisory |

## POST /data/refresh
Refresh all weather data and pages