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

        var sequenceNumber = BitConverter.ToUInt32(data, startIndex + 4);
        var acknowledgementNumber = BitConverter.ToUInt32(data, startIndex + 8);

        var dataOffset = (byte)(data[startIndex + 12] >> 4);
        var flags = (TcpHeaderFlags)data[startIndex + 13];

        var window = data[startIndex + 14] << 8 | data[startIndex + 15];
        var checkSum = data[startIndex + 16] << 8 | data[startIndex + 17];
        var urgentPointer = data[startIndex + 18] << 8 | data[startIndex + 19];

        var header = new TcpHeader(sourcePort, destinationPort, sequenceNumber, acknowledgementNumber,
            dataOffset, flags, window, checkSum, urgentPointer);

        length = header.TcpHeaderLength;

        if (header.OptionsLength != 0)
        {
            var start = startIndex + 20;

            ReadTcpOptions(data, start, header);
        }

        return header;
    }

    private void ReadTcpOptions(byte[] data, int start, TcpHeader header)
    {
        while (true)
        {
            if (!TryParseTcpOption(data, start, out var option, out var nextIndex))
                break;

            start = nextIndex;
            if(option is not null) //option is null for end of options / no op
                header.AddOption(option);
        }
    }

    private bool TryParseTcpOption(byte[] data, int index, out TcpOption option, out int nextIndex)
    {
        option = null;
        nextIndex = index + 1;

        if (data[index] == (byte)TcpOptionsKind.EndOfOptionsList)
            return true;
        else if (data[index] == (byte)TcpOptionsKind.NoOp)
            return true;
        else if (data[index] == (byte)TcpOptionsKind.MaximumSegmentSize)
        {
            option = new TcpOptionMss(data[index + 2]);
            nextIndex = index + option.Length;
            return true;
        }
        else if (data[index] == (byte)TcpOptionsKind.WindowScale)
        {
            option = new TcpOptionMss(data[index + 1]);
            nextIndex = index + option.Length;
            return true;
        }
        else if (data[index] == (byte)TcpOptionsKind.SackPermitted)
        {
            option = new TcpOptionMss(data[index + 1]);
            nextIndex = index + option.Length;
            return true;
        }
        else if (data[index] == (byte)TcpOptionsKind.SACK)
        {
            option = new TcpOptionMss(data[index + 1]);
            nextIndex = index + option.Length;
            return true;
        }
        else if (data[index] == (byte)TcpOptionsKind.TimeStamp)
        {
            option = new TcpOptionMss(data[index + 1]);
            nextIndex = index + option.Length;
            return true;
        }
        else if (data[index] == (byte)TcpOptionsKind.UserTimeoutOption)
        {
            option = new TcpOptionMss(data[index + 1]);
            nextIndex = index + option.Length;
            return true;
        }
        else if (data[index] == (byte)TcpOptionsKind.TcpAuthentication)
        {
            option = new TcpOptionMss(data[index + 1]);
            nextIndex = index + option.Length;
            return true;
        }
        else if (data[index] == (byte)TcpOptionsKind.MultipathTcp)
        {
            option = new TcpOptionMss(data[index + 1]);
            nextIndex = index + option.Length;
            return true;
        }
        else
            throw new InvalidOperationException($"Undefined tcp option type {data[index]}");


    }

    public void Encode(TcpHeader ipHeader, byte[] data, int startIndex, out int length)
        => throw new NotImplementedException();
}
