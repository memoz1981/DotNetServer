using System.Net;

namespace DotNetServer.TCP.IP;
public abstract class IpHeader
{
    protected IpHeader(
        IpVersion version,
        IPAddress sourceAddress,
        IPAddress destinationAddress)
    {
        Version = version;
        SourceAddress = sourceAddress;
        DestinationAddress = destinationAddress;
    }

    public IpVersion Version { get; }
    public IPAddress SourceAddress { get; }
    public IPAddress DestinationAddress { get; }
}
