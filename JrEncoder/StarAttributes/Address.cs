namespace JrEncoder.StarAttributes;

/// <summary>
/// Represents a STAR address.
/// </summary>
/// <param name="serviceId">3 bits service ID.</param>
/// <param name="zone">10 bits zone ID.</param>
/// <param name="county">5 bits county ID.</param>
/// <param name="unit">6 bits unit ID.</param>
public struct Address(int serviceId, int zone, int county, int unit)
{
    private readonly int _serviceId = serviceId;
    private readonly int _zone = zone;
    private readonly int _county = county;
    private readonly int _unit = unit;

    /// <summary>
    /// Returns a 6-byte packet representing the address.
    /// </summary>
    /// <returns></returns>
    private byte[] ToBytes()
    {
        throw new NotImplementedException();
    }
}