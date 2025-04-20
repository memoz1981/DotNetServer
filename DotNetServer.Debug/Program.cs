using System.Net;
using DotNetServer.TCP.IP;
using DotNetServer.TCP.Services;
using DotNetServer.TCP.TCP;

var tcpConnectionManager = new TcpConnectionManager(new PacketListenerService(), new IPv4HeaderParser(), new TcpHeaderParser());

var cancellationToken = new CancellationTokenSource().Token;
var result = tcpConnectionManager.Subcribe(IPAddress.Loopback, 5050, cancellationToken);

await foreach (var item in result)
{
    Console.WriteLine(item);
}
