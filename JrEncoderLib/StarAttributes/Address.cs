using System;
using System.Linq;

namespace JrEncoderLib.StarAttributes;

/// <summary>
/// Represents a STAR address.
/// </summary>
/// <param name="serviceId">3 bits service ID. Always 1 for all STARs</param>
/// <param name="zone">10 bits zone ID.</param>
/// <param name="county">5 bits county ID.</param>
/// <param name="unit">6 bits unit ID.</param>
public struct Address(int serviceId, int zone, int county, int unit)
{
    /// <summary>
    /// Service id 1 and everything else 0s makes this address all stars
    /// </summary>
    public static Address All = new Address(1, 0, 0, 0);

    /// <summary>
    /// Setting service id to 0 makes this address no stars
    /// </summary>
    public static Address None = new Address(0, 0, 0, 0);

    /// <summary>
    /// Returns a 6-byte array representing the address.
    /// </summary>
    /// <returns></returns>
    public byte[] ToBytes()
    {
        byte[] address = new byte[6];

        // [sid]  [z]
        // 0 0 0  0
        address[0] = (byte)((serviceId << 1) | zone & 0b1000000000);

        // [zone ]
        // 0 0 0 0
        address[1] = (byte)((zone & 0b0111100000) >> 5);

        // [zone ]
        // 0 0 0 0
        address[2] = (byte)((zone & 0b0000011110) >> 1);

        // [z][cnty]
        // 0  0 0 0
        address[3] = (byte)(((zone & 0b0000000001) << 3) | ((county & 0b11100) >> 2));

        // [cn][u]
        // 0 0 0 0
        address[4] = (byte)(((county & 0b00000011) << 2) | ((unit & 0b110000) >> 4));

        // [unit ]
        // 0 0 0 0
        address[5] = (byte)(unit & 0b0000001111);

        return address;
    }

    public static Address FromSwitches(string switches)
    {
        if (switches.Length != 8)
            throw new Exception("Switches must be 8 characters long");

        // All zero switches means all stars
        if (switches == "00000000")
            return All;

        byte[] switchBytes = StringToByteArray(switches);

        // zone is the last 5 bits of the 2nd switch, and first 5 bits of the 3rd switch
        int zone = ((switchBytes[1] & 0b00011111) << 5) | (switchBytes[2] >> 3);

        // county is the last 3 bits of the 3rd switch, and the first 2 bits of the 4th switch
        int county = ((switchBytes[2] & 0b00000111) << 2) | (switchBytes[3] >> 6);

        // unit is the last 6 bits of the 4th switch
        int unit = switchBytes[3] & 0b000111111;

        return new Address(1, zone, county, unit);
    }

    public static int GetTimeZone(string switches)
    {
        byte[] switchBytes = StringToByteArray(switches);
        // Extract timezone (first 3 bits of 2nd switch value)
        return switchBytes[1] >> 5;
    }

    private static byte[] StringToByteArray(string hex)
    {
        return Enumerable.Range(0, hex.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            .ToArray();
    }
}