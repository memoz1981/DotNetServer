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
        [EnumeratorCancellation]CancellationToken cancellationToken)
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

        if(ipv4.Protocol != Protocols.TCP)
            return (default, false);

        // extract tcp header
        var tcpHeader = _tcpHeaderParser.Decode(dataReceived.data, tcpStartIndex, out var nextIndex);

        if(tcpHeader.DestinationPort != port)
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

    private TcpConnection CreateConnection(IPv4Header ipHeader, TcpHeader tcpHeader) => throw new NotImplementedException();
}
