﻿namespace DotNetServer.TCP.TCP;
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
        int startIndex = start; 
        while (startIndex - start < header.OptionsLength)
        {
            if (!TryParseTcpOption(data, startIndex, out var option, out var nextIndex))
                break;

            startIndex = nextIndex;
            if(option is not null) //option is null for end of options / no op
                header.AddOption(option);
        }
    }

    //didn't bother much with exception handling here...
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
            var mss = (ushort)(data[index + 2] << 8 | data[index + 3]);
            option = new TcpOptionMss(mss);
            nextIndex = index + option.Length;
            return true;
        }
        else if (data[index] == (byte)TcpOptionsKind.WindowScale)
        {
            option = new TcpOptionWindowScale(data[index + 2]);
            nextIndex = index + option.Length;
            return true;
        }
        else if (data[index] == (byte)TcpOptionsKind.SackPermitted)
        {
            option = new TcpOptionsSackPermitted();
            nextIndex = index + option.Length;
            return true;
        }
        else if (data[index] == (byte)TcpOptionsKind.SACK)
        {
            var blocks = new List<(uint, uint)>();
            var bytesAvailable = data[index + 1] - 2;
            if (bytesAvailable > 0 && bytesAvailable % 8 == 0)
            {
                for (int i = 0; i < bytesAvailable; i += 8)
                {
                    var blockStart = (data[index + i + 2] << 24) |
                        (data[index + i + 3] << 16) | (data[index + i + 4] << 8) | data[index + i + 5];

                    var blockEnd = (data[index + i + 6] << 24) |
                        (data[index + i + 7] << 16) | (data[index + i + 8] << 8) | data[index + i + 9];

                    blocks.Add(((uint)blockStart, (uint)blockEnd));
                }
            }

            option = new TcpOptionsSack(data[index + 1], blocks);
            nextIndex = index + option.Length;
            return true;
        }
        else if (data[index] == (byte)TcpOptionsKind.TimeStamp)
        {
            int i = 0; 
            var tsVal = (data[index + i + 2] << 24) |
                        (data[index + i + 3] << 16) | (data[index + i + 4] << 8) | data[index + i + 5];

            var tsEcr = (data[index + i + 6] << 24) |
                (data[index + i + 7] << 16) | (data[index + i + 8] << 8) | data[index + i + 9];

            option = new TcpOptionsTimestamp((uint)tsVal, (uint)tsEcr);
            nextIndex = index + option.Length;
            return true;
        }
        else if (data[index] == (byte)TcpOptionsKind.UserTimeoutOption)
        {
            var timeoutInMs = data[index + 2] << 8 | data[index + 3];
            option = new TcpOptionUserTimeout((uint)timeoutInMs);
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
