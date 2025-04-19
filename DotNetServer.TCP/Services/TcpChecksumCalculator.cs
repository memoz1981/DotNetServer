// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetServer.TCP.IP;
using DotNetServer.TCP.TCP;

namespace DotNetServer.TCP.Services;
internal static class TcpChecksumCalculator
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
}
