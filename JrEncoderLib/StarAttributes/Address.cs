using System;

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
        address[1] = (byte)(zone & 0b0111100000);
        
        // [zone ]
        // 0 0 0 0
        address[2] = (byte)(zone & 0b0000011110);
        
        // [z][cnty]
        // 0  0 0 0
        address[3] = (byte)((zone & 0b0000000001) | county << 2);
        
        // [cn][u] 
        // 0 0 0 0
        address[4] = (byte)((county & 0b11000) | unit << 4);
        
        // [unit ]
        // 0 0 0 0
        address[5] = (byte)(unit & 0b0000001111);
        
        Console.WriteLine(Convert.ToHexString(address));
        return address;
    }

    public static Address FromSwitches(string switches)
    {
        return new Address(1, 0, 0, 0);
    }
}