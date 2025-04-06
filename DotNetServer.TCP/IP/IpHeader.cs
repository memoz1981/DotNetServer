using System.Net;

namespace DotNetServer.TCP.IP;
public abstract class IpHeader
{
    public IpVersion Version { get; }
    public IPAddress SourceAddress { get; }
    public IPAddress DestinationAddress { get; }
}
