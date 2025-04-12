using DotNetServer.TCP.Extensions;

namespace DotNetServer.TCP.TCP;
public interface ITcpHeaderParser
{
    TcpHeader Decode(byte[] data, int startIndex, out int length);

    void Encode(TcpHeader tcpHeader, byte[] data, int startIndex, out int length);
}

public class TcpHeaderParser : ITcpHeaderParser
{
    public TcpHeader Decode(byte[] data, int startIndex, out int length)
    {
        if (data is null or [] || data.Length < startIndex + 20)
            throw new ArgumentException("Invalid TCP header.");

        var sourcePort = data[startIndex] << 8 | data[startIndex + 1];
        var destinationPort = data[startIndex + 2] << 8 | data[startIndex + 3];

        var sequenceNumber = data.ReadUIntFromBigEndianArray(startIndex + 4); 
        var acknowledgementNumber = data.ReadUIntFromBigEndianArray(startIndex + 8);

        var dataOffset = (byte)(data[startIndex + 12] >> 4);
        var flags = (TcpHeaderFlags)data[startIndex + 13];

        var window = data[startIndex + 14] << 8 | data[startIndex + 15];
        var checkSum = (ushort)(data[startIndex + 16] << 8 | data[startIndex + 17]);
        var urgentPointer = (ushort)(data[startIndex + 18] << 8 | data[startIndex + 19]);

        var header = new TcpHeader(sourcePort, destinationPort, sequenceNumber, acknowledgementNumber,
            dataOffset, flags, window, checkSum, urgentPointer);

        length = header.TcpHeaderLength;

        if (header.OptionsLength != 0)
        {
            var start = startIndex + 20;

            ReadTcpOptions(data, start, header, out var nextIndex);

            if (length != nextIndex)
                throw new InvalidOperationException($"Calculated index {nextIndex} doesn't match with tcp header length {length}...");
        }

        return header;
    }

    private void ReadTcpOptions(byte[] data, int start, TcpHeader header, out int index)
    {
        index = start; 
        int startIndex = start; 
        while (startIndex - start < header.OptionsLength)
        {
            if (!TryParseTcpOption(data, startIndex, out var option, out var nextIndex))
                break;

            index = nextIndex; 

            startIndex = nextIndex;
            if(option is not null) //option is null for end of options / no op
                header.AddOption(option);
        }
    }

    //didn't bother much with exception handling here...
    private bool TryParseTcpOption(byte[] data, int index, out TcpOption option, out int nextIndex)
    {
        if (data[index] == (byte)TcpOptionsKind.EndOfOptionsList)
        {
            option = new TcpOptionNone();
            nextIndex = index + option.Length;
            return true;
        }
        else if (data[index] == (byte)TcpOptionsKind.NoOp)
        {
            option = new TcpOptionNoOp();
            nextIndex = index + option.Length;
            return true;
        }
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

            option = new TcpOptionsSack(blocks);
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
            option = new TcpOptionUserTimeout((ushort)timeoutInMs);
            nextIndex = index + option.Length;
            return true;
        }
        else if (data[index] == (byte)TcpOptionsKind.TcpAuthentication)
        {
            option = new TcpOptionAuthenticated();
            nextIndex = index + option.Length;
            return true;
        }
        else if (data[index] == (byte)TcpOptionsKind.MultipathTcp)
        {
            option = new TcpOptionMultipath();
            nextIndex = index + option.Length;
            return true;
        }
        else
            throw new InvalidOperationException($"Undefined tcp option type {data[index]}");
    }

    public void Encode(TcpHeader tcpHeader, byte[] data, int startIndex, out int length)
    {
        //validations
        if (tcpHeader.TcpHeaderLength + startIndex > data.Length)
            throw new ArgumentException($"Provided data array length not enough {data.Length}, " +
                $"required length: {tcpHeader.TcpHeaderLength + startIndex}");

        var index = startIndex;

        //source port
        data[index++] = (byte)(tcpHeader.SourcePort >> 8);
        data[index++] = (byte)(tcpHeader.SourcePort & 0xFF);

        //destination port
        data[index++] = (byte)(tcpHeader.DestinationPort >> 8);
        data[index++] = (byte)(tcpHeader.DestinationPort & 0xFF);

        //sequence number
        data[index++] = (byte)(tcpHeader.SequenceNumber >> 24);
        data[index++] = (byte)(tcpHeader.SequenceNumber >> 16);
        data[index++] = (byte)(tcpHeader.SequenceNumber >> 8);
        data[index++] = (byte)(tcpHeader.SequenceNumber & 0xFF);

        //acknowledgement number
        data[index++] = (byte)(tcpHeader.AcknowledgementNumber >> 24);
        data[index++] = (byte)(tcpHeader.AcknowledgementNumber >> 16);
        data[index++] = (byte)(tcpHeader.AcknowledgementNumber >> 8);
        data[index++] = (byte)(tcpHeader.AcknowledgementNumber & 0xFF);

        //data offset (&reserved)
        data[index++] = (byte)(tcpHeader.DataOffset << 4 & 0xF0);

        //flags
        data[index++] = (byte)tcpHeader.Flags;

        //window
        data[index++] = (byte)(tcpHeader.Window >> 8);
        data[index++] = (byte)(tcpHeader.Window & 0xFF);

        //checksum
        data[index++] = (byte)(tcpHeader.Checksum >> 8);
        data[index++] = (byte)(tcpHeader.Checksum & 0xFF);

        //urgent pointer
        data[index++] = (byte)(tcpHeader.UrgentPointer >> 8);
        data[index++] = (byte)(tcpHeader.UrgentPointer & 0xFF);

        foreach (var option in tcpHeader.Options)
        {
            EncodeOption(data, ref index, option); 
        }

        length = index;
    }

    private void EncodeOption(byte[] data, ref int index, TcpOption option)
    {
        //write the kind
        data[index++] = (byte)option.Kind;

        switch (option.Kind)
        {
            case TcpOptionsKind.EndOfOptionsList:
            case TcpOptionsKind.NoOp:
                break; //only 1 byte of data

            case TcpOptionsKind.MaximumSegmentSize:
                var optionMss = (TcpOptionMss)option; 
                data[index++] = (byte)optionMss.Length;
                optionMss.MaximumSegmentSize.WriteUShortBigEndian(data, ref index);
                break;

            case TcpOptionsKind.WindowScale:
                var optionWindowScale = (TcpOptionWindowScale)option;
                data[index++] = (byte)option.Length;
                data[index++] = optionWindowScale.WindowScale;
                break;

            case TcpOptionsKind.SackPermitted:
                var optionSackPermitted = (TcpOptionsSackPermitted)option;
                data[index++] = (byte)option.Length;
                break;

            case TcpOptionsKind.SACK:
                var optionSack = (TcpOptionsSack)option;
                data[index++] = (byte)option.Length;
                foreach (var value in optionSack.Blocks)
                {
                    value.Item1.WriteUInt32BigEndian(data, ref index);
                    value.Item2.WriteUInt32BigEndian(data, ref index);
                }
                break;

            case TcpOptionsKind.TimeStamp:
                var optionTimestamp = (TcpOptionsTimestamp)option;
                data[index++] = (byte)option.Length;
                optionTimestamp.TimestampValue.WriteUInt32BigEndian(data, ref index);
                optionTimestamp.TimestampEchoReply.WriteUInt32BigEndian(data, ref index);
                break;

            case TcpOptionsKind.UserTimeoutOption:
                var optionUserTimeout = (TcpOptionUserTimeout)option;
                data[index++] = (byte)option.Length;
                optionUserTimeout.TimeoutInMs.WriteUShortBigEndian(data, ref index);
                break;

            //authenticated and multipart not implemented...

            default:
                throw new InvalidOperationException();
        };
    }
}
