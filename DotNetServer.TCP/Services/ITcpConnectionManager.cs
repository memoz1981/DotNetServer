using System.Collections.Concurrent;
using System.Net;
using DotNetServer.TCP.IP;
using DotNetServer.TCP.TCP;

namespace DotNetServer.TCP.Services;

public interface ITcpConnectionManager
{
    IAsyncEnumerable<byte[]> Subcribe(IPAddress ipAddress, int port, CancellationToken cancellationToken); 
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

    public async IAsyncEnumerable<byte[]> Subcribe(IPAddress ipAddress, int port, CancellationToken cancellationToken)
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

    private async Task<(byte[] httpData, bool shouldReturn)> HandleRequest((byte[] data, int length) dataReceived, IPAddress ipAddress, int port)
    {
        // extract ip headers
        var ipHeader = _ipHeaderParser.Decode(dataReceived.data, out var tcpStartIndex);
        var tcpHeader = _tcpHeaderParser.Decode(dataReceived.data, tcpStartIndex, out var dataLength);

        // check dictionary
        var sourceIp = ipHeader.SourceAddress;
        var sourcePort = tcpHeader.SourcePort;
        var key = new TcpConnectionKey(sourceIp, ipAddress, sourcePort, port);
        if (!_connectionDictionary.TryGetValue(key, out var connection))
            _connectionDictionary[key] = CreateConnection(ipHeader, tcpHeader);

        //extract tcp/ip headers
        var handled = await _connectionDictionary[key].HandleRequest(ipHeader, tcpHeader, dataReceived, dataLength);

        if (handled.shouldReturn)
            return handled;

        return (null, false); 
    }

    private TcpConnection CreateConnection(IpHeader ipHeader, TcpHeader tcpHeader) => throw new NotImplementedException();
}
