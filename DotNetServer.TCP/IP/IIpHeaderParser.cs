using System.Net;

namespace DotNetServer.TCP.IP;
public interface IIpHeaderParser
{
    IpHeader Decode(byte[] data, out int length);
    void Encode(IpHeader ipHeader, byte[] data, int startIndex, out int length); 
}

public class IPv4HeaderParser : IIpHeaderParser
{
    public IpHeader Decode(byte[] data, out int length)
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
        var destinationAddress = new IPAddress(new ArraySegment<byte>(data, 16, 4).ToArray());

        //temproarily passing empty to options...
        var options = Array.Empty<byte>();

        var header = new IPv4Header(version, sourceAddress, destinationAddress,
            internetHeaderLength, differentiatedServicesCodePoint, explicitCongestionNotification,
            totalLength, identification, flags, fragmentOffset, timeToLive, protocol, headerChecksum,
            options);

        length = header.HeaderLength;

        return header;
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
    {
        if (ipHeader is not IPv4Header)
            throw new ArgumentException("This class is IPv4 only...");

        var ipv4 = (IPv4Header)ipHeader;

        var lengthRequired = ipv4.HeaderLength;

        if (ipv4.TotalLength < data.Length - startIndex)
            throw new InvalidOperationException($"Total data length of {ipv4.TotalLength} not sufficient in provided byte array...");

        var index = startIndex;

        //version and ihl
        data[index] = (byte)(((int)ipv4.Version << 4) | (ipv4.InternetHeaderLength & 0x0F));

        //dscp and ecn
        data[++index] = (byte)(((int)ipv4.DifferentiatedServicesCodePoint << 6) | (ipv4.ExplicitCongestionNotification & 0x03));

        //total length
        data[++index] = (byte)((ipv4.TotalLength >> 8) & 0xFF); // High byte

        data[++index] = (byte)(ipv4.TotalLength & 0xFF); // Low byte

        //identification
        data[++index] = (byte)((ipv4.Identification >> 8) & 0xFF); // High byte
        data[++index] = (byte)(ipv4.Identification & 0xFF); // Low byte

        //flags and fragment offset
        data[++index] = (byte)((byte)ipv4.Flags | (ipv4.FragmentOffset >> 8 & 0x1F));
        data[++index] = (byte)(ipv4.FragmentOffset & 0xFF);

        //time to live
        data[++index] = ipv4.TimeToLive;

        //protocol
        data[++index] = (byte)ipv4.Protocol;

        //header checksum
        data[++index] = (byte)((ipv4.HeaderChecksum >> 8) & 0xFF); // High byte
        data[++index] = (byte)(ipv4.HeaderChecksum & 0xFF); // Low byte

        //source address
        byte[] sourceAddressBytes = ipv4.SourceAddress.GetAddressBytes(); // Returns bytes in correct order
        Buffer.BlockCopy(sourceAddressBytes, 0, data, ++index, 4);
        index += 4;

        //destination address
        byte[] destAddressBytes = ipv4.DestinationAddress.GetAddressBytes(); // Returns bytes in correct order
        Buffer.BlockCopy(destAddressBytes, 0, data, index, 4);
        index += 4;

        length = index - startIndex;
    }
}
