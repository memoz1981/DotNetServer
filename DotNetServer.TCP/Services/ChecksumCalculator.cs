using System.Collections.Generic;
using DotNetServer.TCP.IP;
using DotNetServer.TCP.TCP;

namespace DotNetServer.TCP.Services;
internal static class ChecksumCalculator
{
    public static ushort ComputeTcpChecksumSafe(
    IPv4Header ipHeader,
    TcpHeader tcpHeader,
    ReadOnlySpan<byte> payload)
    {
        uint sum = 0;

        // --- Pseudo Header (Safe IP handling) ---
        byte[] srcIp = ipHeader.SourceAddress.GetAddressBytes();
        byte[] dstIp = ipHeader.DestinationAddress.GetAddressBytes();

        sum += (uint)((srcIp[0] << 8) | srcIp[1]);
        sum += (uint)((srcIp[2] << 8) | srcIp[3]);
        sum += (uint)((dstIp[0] << 8) | dstIp[1]);
        sum += (uint)((dstIp[2] << 8) | dstIp[3]);

        sum += 6; // Protocol: TCP (6)

        int tcpLength = tcpHeader.TcpHeaderLength + payload.Length;
        sum += (uint)((tcpLength >> 8) & 0xFF);
        sum += (uint)(tcpLength & 0xFF);

        // --- TCP Header Fields ---
        sum += (uint)((tcpHeader.SourcePort >> 8) & 0xFF) << 8 | (uint)(tcpHeader.SourcePort & 0xFF);
        sum += (uint)((tcpHeader.DestinationPort >> 8) & 0xFF) << 8 | (uint)(tcpHeader.DestinationPort & 0xFF);

        sum += (uint)((tcpHeader.SequenceNumber >> 16) & 0xFFFF);
        sum += (uint)(tcpHeader.SequenceNumber & 0xFFFF);

        sum += (uint)((tcpHeader.AcknowledgementNumber >> 16) & 0xFFFF);
        sum += (uint)(tcpHeader.AcknowledgementNumber & 0xFFFF);

        // Offset (4 bits) + Reserved (3 bits) + Flags (9 bits)
        ushort offsetAndFlags = (ushort)(((tcpHeader.DataOffset & 0xF) << 12) | ((ushort)tcpHeader.Flags & 0x01FF));
        sum += offsetAndFlags;

        sum += (uint)tcpHeader.Window;
        sum += 0; // checksum field zeroed during calculation
        sum += tcpHeader.UrgentPointer;

        // --- Payload ---
        for (int i = 0; i < payload.Length; i += 2)
        {
            ushort word = (ushort)(payload[i] << 8);
            if (i + 1 < payload.Length)
                word |= payload[i + 1];
            sum += word;
        }

        // --- Final Fold & One's Complement ---
        while ((sum >> 16) != 0)
            sum = (sum & 0xFFFF) + (sum >> 16);

        return (ushort)~sum;
    }

    public static int CalculateChecksum(IPv4Header header)
    {
        // Only supports 20-byte header (no options)
        ushort[] fields = new ushort[10];

        // First 2 bytes: Version (4 bits) + IHL (4 bits), DSCP + ECN
        fields[0] = (ushort)(((byte)header.Version << 12) | ((header.InternetHeaderLength & 0x0F) << 8) |
                             (header.DifferentiatedServicesCodePoint & 0xFC) | (header.ExplicitCongestionNotification & 0x03));

        // Total Length
        fields[1] = (ushort)header.TotalLength;

        // Identification
        fields[2] = (ushort)header.Identification;

        // Flags + Fragment Offset
        fields[3] = (ushort)(((ushort)header.Flags << 13) | (header.FragmentOffset & 0x1FFF));

        // TTL + Protocol
        fields[4] = (ushort)((header.TimeToLive << 8) | (byte)header.Protocol);

        // Checksum is 0 for calculation
        fields[5] = 0;

        // Source Address
        byte[] sourceBytes = header.SourceAddress.GetAddressBytes();
        fields[6] = (ushort)((sourceBytes[0] << 8) | sourceBytes[1]);
        fields[7] = (ushort)((sourceBytes[2] << 8) | sourceBytes[3]);

        // Destination Address
        byte[] destBytes = header.DestinationAddress.GetAddressBytes();
        fields[8] = (ushort)((destBytes[0] << 8) | destBytes[1]);
        fields[9] = (ushort)((destBytes[2] << 8) | destBytes[3]);

        // Calculate sum
        uint sum = 0;
        for (int i = 0; i < fields.Length; i++)
        {
            sum += fields[i];
        }

        // Fold 32-bit sum to 16 bits
        while ((sum >> 16) != 0)
            sum = (sum & 0xFFFF) + (sum >> 16);

        return (int)~sum;
    }


    public static ushort CalculateTcpChecksum(IPv4Header ipHeader, TcpHeader tcpHeader, byte[] tcpPayload)
    {
        // Create pseudo header
        var pseudoHeader = new List<byte>();

        // Source Address (4 bytes)
        pseudoHeader.AddRange(ipHeader.SourceAddress.GetAddressBytes());

        // Destination Address (4 bytes)
        pseudoHeader.AddRange(ipHeader.DestinationAddress.GetAddressBytes());

        // Zero byte and Protocol (1 + 1 = 2 bytes)
        pseudoHeader.Add(0);
        pseudoHeader.Add((byte)ipHeader.Protocol);

        // TCP Length (2 bytes) - header + payload
        ushort tcpLength = (ushort)(tcpHeader.DataOffset * 4 + tcpPayload.Length);
        pseudoHeader.AddRange(BitConverter.GetBytes(tcpLength).ReverseIfLittleEndian());

        // Create TCP header without checksum
        var tcpHeaderBytes = new List<byte>();

        // Source Port (2 bytes)
        tcpHeaderBytes.AddRange(BitConverter.GetBytes((ushort)tcpHeader.SourcePort).ReverseIfLittleEndian());

        // Destination Port (2 bytes)
        tcpHeaderBytes.AddRange(BitConverter.GetBytes((ushort)tcpHeader.DestinationPort).ReverseIfLittleEndian());

        // Sequence Number (4 bytes)
        tcpHeaderBytes.AddRange(BitConverter.GetBytes(tcpHeader.SequenceNumber).ReverseIfLittleEndian());

        // Acknowledgement Number (4 bytes)
        tcpHeaderBytes.AddRange(BitConverter.GetBytes(tcpHeader.AcknowledgementNumber).ReverseIfLittleEndian());

        // Data Offset and Reserved (1 byte) and Flags (1 byte)
        byte dataOffsetAndReserved = (byte)((tcpHeader.DataOffset << 4) | 0);
        tcpHeaderBytes.Add(dataOffsetAndReserved);
        tcpHeaderBytes.Add((byte)tcpHeader.Flags);

        // Window (2 bytes)
        tcpHeaderBytes.AddRange(BitConverter.GetBytes((ushort)tcpHeader.Window).ReverseIfLittleEndian());

        // Checksum (2 bytes) - zero for calculation
        tcpHeaderBytes.AddRange(new byte[] { 0, 0 });

        // Urgent Pointer (2 bytes)
        tcpHeaderBytes.AddRange(BitConverter.GetBytes(tcpHeader.UrgentPointer).ReverseIfLittleEndian());

        // Options (variable length, padded to 32-bit boundary)
        if (tcpHeader.Options != null && tcpHeader.Options.Count > 0)
        {
            //tcpHeaderBytes.AddRange(tcpHeader.Options);
            //// Pad options to make header length a multiple of 4
            //int padding = (tcpHeader.DataOffset * 4) - (20 + tcpHeader.Options.Count);
            //for (int i = 0; i < padding; i++)
            //{
            //    tcpHeaderBytes.Add(0);
            //}
        }

        // Combine all parts for checksum calculation
        var checksumData = new List<byte>();
        checksumData.AddRange(pseudoHeader);
        checksumData.AddRange(tcpHeaderBytes);
        checksumData.AddRange(tcpPayload);

        // If the data length is odd, add a padding byte
        if (checksumData.Count % 2 != 0)
        {
            checksumData.Add(0);
        }

        return CalculateChecksum(checksumData.ToArray());
    }

    private static ushort CalculateChecksum(byte[] data)
    {
        uint sum = 0;

        for (int i = 0; i < data.Length; i += 2)
        {
            ushort word = (ushort)((data[i] << 8) + (i + 1 < data.Length ? data[i + 1] : 0));
            sum += word;

            // Wrap around carry bits
            if ((sum & 0xFFFF0000) != 0)
            {
                sum = (sum & 0xFFFF) + (sum >> 16);
            }
        }

        return (ushort)~sum;
    }

    private static byte[] ReverseIfLittleEndian(this byte[] bytes)
    {
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return bytes;
    }
}
