using JrEncoder.StarAttributes;

namespace JrEncoder.Frames;

public class TimeOfDayFrame : ControlFrame
{
    /// <summary>
    /// Creates a new time of day DataFrame with the given parameters.
    /// </summary>
    /// <param name="omcw">The OMCW to use when creating this frame.</param>
    /// <param name="timeZone">The time zone.
    /// TODO: Document this better.
    /// </param>
    /// <param name="dayOfWeek">The day of the week.</param>
    /// <param name="month">The month of the year.</param>
    /// <param name="dayOfMonth">The day of the month.</param>
    /// <param name="hours">The hour of the day. 0 - 12</param>
    /// <param name="minutes">The minute of the hour.</param>
    /// <param name="seconds">The second of the minute.</param>
    /// <param name="isPM">Whether it is AM or PM.</param>
    public TimeOfDayFrame(OMCW omcw, int timeZone, DayOfWeek dayOfWeek, int month, int dayOfMonth, int hours, int minutes, int seconds, bool isPM) {
        this.omcw = omcw;

        /*
        Prior Reference: https://patentimages.storage.googleapis.com/6d/b2/60/69fee298647dc3/US4725886.pdf
        Reference: https://patentimages.storage.googleapis.com/8d/f3/42/7f8952923cce48/US4916539.pdf
        Note: Alternate time settings seem to be unused on most units. 
        Some units could have the "Alternate Time" Switch/Flag set, in which case only alternate time will work.
        */
        frame[0] = ClockRunIn;
        frame[1] = ClockRunIn;
        frame[2] = FramingCode;
        frame[3] = 0;  // Row Number: 0
        // 4-7 OMCW
        frame[8] = 0;  // Page Number: 0 for TOD
        frame[9] = 0;  // "
        frame[10] = (byte) timeZone; // Timezone
        frame[11] = (byte) dayOfWeek; // Day of Week
        frame[12] = (byte) month; // Month
        frame[13] = (byte) ((dayOfMonth >> 4) & 0x0F); // Day of Month
        frame[14] = (byte) (dayOfMonth & 0x0F); // "
        frame[15] = (byte) hours; // Hours (0-12)
        frame[16] = (byte) ((minutes >> 4) & 0x0F); // Minutes
        frame[17] = (byte) (minutes & 0x0F); // "
        frame[18] = (byte) ((seconds >> 4) & 0x0F); // Seconds
        frame[19] = (byte) (seconds & 0x0F); // "
        frame[20] = (byte) (isPM ? 1 : 0);
        frame[21] = 0; // Alt. Day of Week
        frame[22] = 0; // Alt. Month
        frame[23] = 0; // Alt. Day of Month
        frame[24] = 0; // "
        frame[25] = 0; // Alt. Hours
        frame[26] = 0; // Alt. Minutes
        frame[27] = 0; // "
        frame[28] = 0; // Alt. Seconds
        frame[29] = 0; // "
        frame[30] = 0; // Alt. AM/PM
        frame[31] = 0; // Checksum
        frame[32] = 0; // "
        frame[33] = 0; // OMCW Extension
        frame[34] = 0; // "
        frame[35] = 0; // Spare
        frame[36] = 0; // Spare

        // Set bytes 4-7 for OMCW
        setOmcwBytes();

        // This packet contains a checksum, which is just indexes 10-30 summed
        byte checksum = 0;
        for (int i = 10; i <= 30; i++)
            checksum += frame[i];

        // Split the checksum into separate nibbles
        frame[31] = (byte) ((checksum >> 4) & 0x0F); // High byte
        frame[32] = (byte) (checksum & 0x0F); // Low byte

        // Convert bytes 3-36 to hamming code
        hamBytes(3, 36);
    }

    /// <summary>
    /// Creates a TimeOfDayFrame using the current time as the starting timestamp.
    /// </summary>
    /// <param name="omcw"></param>
    /// <param name="timeZone"></param>
    /// <returns></returns>
    public static TimeOfDayFrame Now(OMCW omcw, int timeZone)
    {
        DateTime currentTime = DateTime.Now;
        return new TimeOfDayFrame(
            omcw,
            timeZone,
            currentTime.DayOfWeek,
            currentTime.Month,
            currentTime.Day,
            currentTime.Hour % 12,
            currentTime.Minute,
            currentTime.Second,
            currentTime.Hour > 12
        );
    }
}