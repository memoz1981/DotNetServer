using System.Collections.Concurrent;
using System.Net;
using System.Runtime.CompilerServices;
using DotNetServer.TCP.IP;
using DotNetServer.TCP.TCP;

namespace DotNetServer.TCP.Services;

public interface ITcpConnectionManager
{
    IAsyncEnumerable<HttpData> Subcribe(IPAddress ipAddress, int port, CancellationToken cancellationToken);
    Task Send(HttpData httpData); 
}

public class TcpConnectionManager : ITcpConnectionManager
{
    private ConcurrentDictionary<TcpConnectionKey, TcpConnection> _connectionDictionary = new();
    private readonly IPacketListenerService _listenerService;
    private readonly IIpHeaderParser _ipHeaderParser;
    private readonly ITcpHeaderParser _tcpHeaderParser;

    public TcpConnectionManager(IPacketListenerService listenerService)
    {
        _listenerService = listenerService;
    }

    public async IAsyncEnumerable<HttpData> Subcribe(IPAddress ipAddress, int port,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var dataReceived in _listenerService.Subscribe(ipAddress, port, cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var result = await HandleRequest(dataReceived, ipAddress, port);

            if (result.shouldReturn)
                yield return result.httpData;
        }
    }

    public async Task Send(HttpData httpData) => throw new NotImplementedException();

    private async Task<(HttpData httpData, bool shouldReturn)> HandleRequest(BufferData dataReceived, IPAddress ipAddress, int port)
    {
        //validate data length
        if (dataReceived.length < 40)
            return (default, false);

        // extract ip header
        var ipHeader = _ipHeaderParser.Decode(dataReceived.data, out var tcpStartIndex);

        if (ipHeader is not IPv4Header)
            return (default, false);

        var ipv4 = (IPv4Header)ipHeader;

        if (ipv4.Protocol != Protocols.TCP)
            return (default, false);

        // extract tcp header
        var tcpHeader = _tcpHeaderParser.Decode(dataReceived.data, tcpStartIndex, out var nextIndex);

        if (tcpHeader.DestinationPort != port)
            return (default, false);

        // check dictionary
        var sourceIp = ipv4.SourceAddress;
        var sourcePort = tcpHeader.SourcePort;

        var key = new TcpConnectionKey(sourceIp, ipAddress, sourcePort, port);
        if (!_connectionDictionary.TryGetValue(key, out var connection))
            _connectionDictionary[key] = CreateConnection(ipv4, tcpHeader);

        //handle
        int? dataIndexStart = nextIndex >= dataReceived.length ? null : nextIndex;
        var tcpData = new TcpData(ipv4, tcpHeader, dataReceived, dataIndexStart);
        var result = await _connectionDictionary[key].HandleRequest(tcpData);

        return result;
    }

    private TcpConnection CreateConnection(IPv4Header ipHeader, TcpHeader tcpHeader)
    {
        var connectionKey = new TcpConnectionKey(ipHeader.SourceAddress, ipHeader.DestinationAddress,
            tcpHeader.SourcePort, tcpHeader.DestinationPort);

        if ((byte)tcpHeader.Flags != 0x010)
            throw new ArgumentException($"Expect a SYN package on new connection, but was {tcpHeader.Flags}");

        var mssOption = tcpHeader.Options.SingleOrDefault(op => op.Kind == TcpOptionsKind.MaximumSegmentSize) as TcpOptionMss;

        var windowScaleOption = tcpHeader.Options.SingleOrDefault(op => op.Kind == TcpOptionsKind.WindowScale) as TcpOptionWindowScale;

        var sackPermitted = tcpHeader.Options.Any(op => op.Kind == TcpOptionsKind.SackPermitted);

        var sackOption = tcpHeader.Options.SingleOrDefault(op => op.Kind == TcpOptionsKind.SACK) as TcpOptionsSack;

        var timeStampOption = tcpHeader.Options.SingleOrDefault(op => op.Kind == TcpOptionsKind.TimeStamp) as TcpOptionsTimestamp;

        var userTimeoutOption = tcpHeader.Options.SingleOrDefault(op => op.Kind == TcpOptionsKind.UserTimeoutOption) as TcpOptionUserTimeout;

        // this should also trigger sending a Syn-Ack message back to client...
        return new TcpConnection(connectionKey, TcpConnectionState.SynReceived, mssOption?.MaximumSegmentSize,
            windowScaleOption?.WindowScale, sackPermitted, sackOption?.Blocks,
            timeStampOption?.TimestampValue, timeStampOption?.TimestampEchoReply, userTimeoutOption?.TimeoutInMs); 
    }
}
