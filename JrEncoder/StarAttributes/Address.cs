namespace JrEncoder.StarAttributes;

public class Address
{
    private int serviceID;
    private int zone;
    private int county;
    private int unit;
    private byte[] address;

    public Address(int serviceID, int zone, int county, int unit) {
        this.serviceID = serviceID;
        this.zone = zone;
        this.county = county;
        this.unit = unit;
    }

    public Address() {
        //Default constructor. Zero out address if called.
        serviceID = 0;
        zone = 0;
        county = 0;
        unit = 0;
    }

    private byte[] createAddressArray() {
        /*
        "Service" (3 bits), "Zone' (10 bits), "County' (5 bits), and "Unit" (6 bits)
        */
        address = new byte[6];
        return address;
    }
}