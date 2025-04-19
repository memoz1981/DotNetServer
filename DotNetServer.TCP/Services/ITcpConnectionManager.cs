using System.Collections.Concurrent;
using System.Net;
using System.Runtime.CompilerServices;
using DotNetServer.TCP.IP;
using DotNetServer.TCP.TCP;

namespace DotNetServer.TCP.Services;

public interface ITcpConnectionManager
{
    IAsyncEnumerable<TcpProcessingContext> Subcribe(IPAddress ipAddress, int port, CancellationToken cancellationToken);
    Task Send(TcpProcessingContext data);
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

    public async IAsyncEnumerable<TcpProcessingContext> Subcribe(IPAddress ipAddress, int port,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var dataReceived in _listenerService.Subscribe(ipAddress, port, cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var context = await HandleRequest(dataReceived, ipAddress, port);

            if (context.ContainsData())
                yield return context; // send to upper environment listeners (Http etc.)
            else if (context.ShouldReturnToClient())
                await Send(context);
            else if (context.DropConnection)
                _connectionDictionary.Remove(context.GetKey(), out var _);
            else
                continue; 
        }
    }

    private async Task<TcpProcessingContext> HandleRequest(BufferData dataReceived, IPAddress ipAddress, int port)
    {
        //validate data length
        if (dataReceived.Length < 40)
            return TcpProcessingContext.Default;

        // extract ip header
        var ipHeader = _ipHeaderParser.Decode(dataReceived.Data, out var tcpStartIndex);

        if (ipHeader is not IPv4Header)
            return TcpProcessingContext.Default;

        var ipv4 = (IPv4Header)ipHeader;

        if (ipv4.Protocol != Protocols.TCP)
            return TcpProcessingContext.Default;

        // extract tcp header
        var tcpHeader = _tcpHeaderParser.Decode(dataReceived.Data, tcpStartIndex, out var nextIndex);

        if (tcpHeader.DestinationPort != port)
            return TcpProcessingContext.Default;

        // check dictionary
        var sourceIp = ipv4.SourceAddress;
        var sourcePort = tcpHeader.SourcePort;

        var key = new TcpConnectionKey(sourceIp, ipAddress, sourcePort, port);
        if (!_connectionDictionary.TryGetValue(key, out var connection))
            _connectionDictionary[key] = CreateConnection(ipv4, tcpHeader);

        //handle
        var dataReceivedWithAmendedIndex = dataReceived with { DataStartIndex = nextIndex };

        var tcpProcessingContext = new TcpProcessingContext(tcpHeader, ipHeader, dataReceivedWithAmendedIndex);
        return await _connectionDictionary[key].Receive(tcpProcessingContext);
    }

    private TcpConnection CreateConnection(IPv4Header ipHeader, TcpHeader tcpHeader)
    {
        var connectionKey = new TcpConnectionKey(ipHeader.SourceAddress, ipHeader.DestinationAddress,
            tcpHeader.SourcePort, tcpHeader.DestinationPort);

        var mssOption = tcpHeader.Options.SingleOrDefault(op => op.Kind == TcpOptionsKind.MaximumSegmentSize) as TcpOptionMss;

        var windowScaleOption = tcpHeader.Options.SingleOrDefault(op => op.Kind == TcpOptionsKind.WindowScale) as TcpOptionWindowScale;

        var sackPermitted = tcpHeader.Options.Any(op => op.Kind == TcpOptionsKind.SackPermitted);

        var sackOption = tcpHeader.Options.SingleOrDefault(op => op.Kind == TcpOptionsKind.SACK) as TcpOptionsSack;

        var timeStampOption = tcpHeader.Options.SingleOrDefault(op => op.Kind == TcpOptionsKind.TimeStamp) as TcpOptionsTimestamp;

        var userTimeoutOption = tcpHeader.Options.SingleOrDefault(op => op.Kind == TcpOptionsKind.UserTimeoutOption) as TcpOptionUserTimeout;

        // this should also trigger sending a Syn-Ack message back to client...
        return new TcpConnection(this, connectionKey, mssOption?.MaximumSegmentSize,
            windowScaleOption?.WindowScale, sackPermitted, sackOption?.Blocks,
            timeStampOption?.TimestampValue, timeStampOption?.TimestampEchoReply, userTimeoutOption?.TimeoutInMs);
    }

    public async Task Send(TcpProcessingContext context)
    {
        // first check if the tcp connection exists
        var tcpConnectionKey = context.GetKey(); 
        if (!_connectionDictionary.TryGetValue(tcpConnectionKey, out var connection))
            throw new InvalidOperationException("No active connections to specified source ip/port..."); //include details

        var dataToSend = _connectionDictionary[tcpConnectionKey].Send(context);

        await foreach (var contextData in dataToSend)
        {
            byte[] serialized = SerializeDataToSend(contextData);

            var ipEndPoint = new IPEndPoint(tcpConnectionKey.SourceIpAddress, tcpConnectionKey.SourcePort);
            await _listenerService.SendAsync(serialized, ipEndPoint);
        }
    }

    private byte[] SerializeDataToSend(TcpProcessingContext context)
    {
        var ipHeader = context.IpHeaderSent as IPv4Header;
        var tcpHeader = context.TcpHeaderSent;
        var data = context.Sent;
        var dataLength = context.Sent?.Length ?? 0;
        var startIndex = context.Sent?.DataStartIndex ?? 0;

        // we expect ip header total length to be correctly set by the TcpConnection... 
        var buffer = new byte[ipHeader.TotalLength];

        _ipHeaderParser.Encode(ipHeader, buffer, 0, out var tcpStartIndex);
        _tcpHeaderParser.Encode(tcpHeader, buffer, tcpStartIndex, out var dataStartIndex);

        //validate data length
        if (ipHeader.TotalLength != dataLength + dataStartIndex)
            throw new InvalidOperationException($"Data start index {dataStartIndex} + data length {dataLength} " +
                $"doesn't match with ip header total length {ipHeader.TotalLength}");

        if(dataLength != 0)
            Array.Copy(data.Value.Data, startIndex, buffer, dataStartIndex, dataLength);

        return buffer;
    }
}
