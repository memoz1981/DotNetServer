using System.Net;

namespace DotNetServer.TCP.IP;
public interface IIpHeaderParser
{
    IpHeader Decode(byte[] data);

    void Encode(IpHeader ipHeader, byte[] data, int startIndex, out int length); 
}

public class IPv4HeaderParser : IIpHeaderParser
{
    public IpHeader Decode(byte[] data)
    {
        if (data is null || data.Length < 20)
            throw new ArgumentException("Packet either null or size less than minimum IP byte size (20).");

        var version = GetVersion(data[0]);

        if (version != IpVersion.IPv4)
            throw new InvalidOperationException("This class is meant for IPv4 packages only."); 

        byte internetHeaderLength = (byte)(data[0] & 0x0F);

        var differentiatedServicesCodePoint = (byte)(data[1] >> 2);
        var explicitCongestionNotification = (byte)(data[1] & 0x03);

        var totalLength = (data[2] << 8) | data[3];
        var identification = (data[4] << 8) | data[5];

        var flags = ReturnFragmentationFlags(data[6]);

        var fragmentOffset = ((data[6] << 8) | data[7]) & 0x1FFF;
        var timeToLive = data[8];

        Protocols protocol = (Protocols)data[9];

        var headerChecksum = (data[10] << 8) | data[11];

        var sourceAddress = new IPAddress(new ArraySegment<byte>(data, 12, 4).ToArray());
        var destinationAddress = new IPAddress(new ArraySegment<byte>(data, 14, 4).ToArray());

        //temproarily passing empty to options...
        var options = Array.Empty<byte>();

        return new(); 
    }

    private IpFragmentationFlags ReturnFragmentationFlags(byte sixthByte)
    {
        byte flags = (byte)(sixthByte >> 5);

        return flags switch {
            0 => IpFragmentationFlags.None,
            1 => IpFragmentationFlags.MoreFragments,
            2 => IpFragmentationFlags.DontFragment,
            _ => throw new InvalidOperationException("Both fragmentation flags cannot be set.")
        };
    }

    private IpVersion GetVersion(byte firstByte)
    {
        var version = (firstByte >> 4) & 0x0F;

        return version switch
        {
            4 => IpVersion.IPv4,
            6 => IpVersion.IPv6,
            _ => throw new ArgumentException($"Unknown IP version {version}.")
        }; 
    }


    public void Encode(IpHeader ipHeader, byte[] data, int startIndex, out int length)
        => throw new NotImplementedException();
}
