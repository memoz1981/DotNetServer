using DotNetServer.TCP.IP;

namespace DotNetServer.TCP.TCP;
public interface ITcpHeaderParser
{
    TcpHeader Decode(byte[] data, int startIndex, out int length);

    void Encode(TcpHeader ipHeader, byte[] data, int startIndex, out int length);
}

public class TcpHeaderParser : ITcpHeaderParser
{
    public TcpHeader Decode(byte[] data, int startIndex, out int length)
    {
        if (data is null or [] || data.Length < startIndex + 20)
            throw new ArgumentException("Invalid TCP header.");

        var sourcePort = data[startIndex] << 8 | data[startIndex + 1];
        var destinationPort = data[startIndex+2] << 8 | data[startIndex + 3];

        var sequenceNumber = BitConverter.ToInt32(data, startIndex + 4);
        var acknowledgementNumber = BitConverter.ToInt32(data, startIndex + 8);

        var dataOffset = (byte)(data[startIndex + 12] >> 4);
        var flags = (TcpHeaderFlags)data[startIndex + 13];

        var window = data[startIndex + 14] << 8 | data[startIndex + 15];
        var checkSum = data[startIndex + 16] << 8 | data[startIndex + 17];
        var urgentPointer = data[startIndex + 18] << 8 | data[startIndex + 19];

        var header = new TcpHeader(sourcePort, destinationPort, sequenceNumber, acknowledgementNumber,
            dataOffset, flags, window, checkSum, urgentPointer);
        length = header.TcpHeaderLength;

        return header;
    }

    public void Encode(TcpHeader ipHeader, byte[] data, int startIndex, out int length)
        => throw new NotImplementedException();
}
