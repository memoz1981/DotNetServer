﻿using DotNetServer.TCP.IP;

namespace DotNetServer.TCP.Services;

//for now this service assumed one packet per request, later will be extended...
// check if it needs to be disposable
public class TcpConnection
{
    public TcpConnection(
        ITcpConnectionManager connectionManager,
        TcpConnectionKey key,
        ushort? maximumSegmentSize,
        byte? windowScale,
        bool sackPermitted,
        List<(uint, uint)> sackBlocks,
        uint? timestampValue,
        uint? timestampEchoReply,
        ushort? timeoutInMs,
        TcpConnectionState state = TcpConnectionState.None)
    {
        _key = key;
        _state = state;
        _maximumSegmentSize = maximumSegmentSize;
        _windowScale = windowScale;
        _sackPermitted = sackPermitted;
        _sackBlocks = sackBlocks;
        _timestampValue = timestampValue;
        _timestampEchoReply = timestampEchoReply;
        _timeoutInMs = timeoutInMs;
        _connectionManager = connectionManager;
    }

    private readonly TcpConnectionKey _key;
    private TcpConnectionState _state;
    private ushort? _maximumSegmentSize;
    private byte? _windowScale;
    private bool? _sackPermitted;
    private List<(uint, uint)> _sackBlocks;
    private uint? _timestampValue;
    private uint? _timestampEchoReply;
    private ushort? _timeoutInMs;
    private readonly ITcpConnectionManager _connectionManager;

    /// <summary>
    /// Updates Tcp connection state and sets values
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task Receive(TcpProcessingContext context)
    {
        if (context.TcpHeaderReceived.Flags == TCP.TcpHeaderFlags.SYN)
        {
            if (_state == TcpConnectionState.None)
                _state = TcpConnectionState.SynReceived; 

            
        }

        
    }

    /// <summary>
    /// Prepares packets to be sent back to client. 
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async IAsyncEnumerable<TcpProcessingContext> Send(TcpProcessingContext context)
    {
        await Task.CompletedTask;

        yield break; 
    }

    private async Task SendSynAck(TcpProcessingContext context)
    {
        //var ipHeader = ReturnSwappedIpv4Header((IPv4Header)tcpData.IpHeader, -1);
        //var tcpHeader = new TcpHeader(
        //    sourcePort: tcpData.TcpHeader.DestinationPort,
        //    destinationPort: tcpData.TcpHeader.SourcePort,
        //    sequenceNumber: 6500, //should be a random number
        //    acknowledgementNumber: tcpData.TcpHeader.SequenceNumber + 1,
        //    dataOffset: tcpData.TcpHeader.DataOffset,
        //    flags: TcpHeaderFlags.SYN | TcpHeaderFlags.ACK,
        //    window: tcpData.TcpHeader.Window,
        //    checksum: 1,
        //    urgentPointer: tcpData.TcpHeader.UrgentPointer);

        //foreach (var option in tcpData.TcpHeader.Options)
        //    tcpHeader.AddOption(option);

        await Task.CompletedTask; 
    }

    private IPv4Header ReturnSwappedIpv4Header(IPv4Header ipHeader, int totalLength) => new IPv4Header(version: ipHeader.Version, sourceAddress: ipHeader.DestinationAddress,
        destinationAddress: ipHeader.SourceAddress, ipHeader.InternetHeaderLength, ipHeader.DifferentiatedServicesCodePoint, ipHeader.ExplicitCongestionNotification,
        totalLength, ipHeader.Identification, ipHeader.Flags, ipHeader.FragmentOffset, ipHeader.TimeToLive, ipHeader.Protocol,
        ipHeader.HeaderChecksum, ipHeader.Options);
}
