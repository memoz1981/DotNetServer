using System.Net;

namespace DotNetServer.TCP.Services;

public record struct TcpConnectionKey
{
    public TcpConnectionKey(IPAddress sourceIpAddress, IPAddress destinationIpAddress, int sourcePort, int destinationPort)
    {
        SourceIpAddress = sourceIpAddress;
        DestinationIpAddress = destinationIpAddress;
        SourcePort = sourcePort;
        DestinationPort = destinationPort;
    }

    public IPAddress SourceIpAddress { get; }
    public IPAddress DestinationIpAddress { get; }
    public int SourcePort { get; }
    public int DestinationPort { get; }
}
