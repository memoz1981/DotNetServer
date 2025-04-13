using System.Collections.Concurrent;
using System.Net;
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

    public async IAsyncEnumerable<HttpData> Subcribe(IPAddress ipAddress, int port, CancellationToken cancellationToken)
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
        // extract ip headers
        var ipHeader = _ipHeaderParser.Decode(dataReceived.data, out var tcpStartIndex);
        var tcpHeader = _tcpHeaderParser.Decode(dataReceived.data, tcpStartIndex, out var nextIndex);

        // check dictionary
        var sourceIp = ipHeader.SourceAddress;
        var sourcePort = tcpHeader.SourcePort;
        var key = new TcpConnectionKey(sourceIp, ipAddress, sourcePort, port);
        if (!_connectionDictionary.TryGetValue(key, out var connection))
            _connectionDictionary[key] = CreateConnection(ipHeader, tcpHeader);

        //add validations (port, ip, tcp etc.)

        //handle
        int? dataIndexStart = nextIndex >= dataReceived.length ? null : nextIndex;
        var tcpData = new TcpData(ipHeader, tcpHeader, dataReceived, dataIndexStart); 
        var handled = await _connectionDictionary[key].HandleRequest(tcpData);

        if (handled.shouldReturn)
            return handled;

        return (default, false); 
    }

    private TcpConnection CreateConnection(IpHeader ipHeader, TcpHeader tcpHeader) => throw new NotImplementedException();
}
