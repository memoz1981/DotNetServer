using DotNetServer.TCP.IP;
using DotNetServer.TCP.TCP;

namespace DotNetServer.TCP.Services;
public record struct TcpProcessingContext
{
    public TcpProcessingContext(
        TcpHeader tcpHeaderReceived,
        IpHeader ipHeaderReceived,
        BufferData received,
        TcpHeader tcpHeaderSent = null,
        IpHeader ipHeaderSent = null,
        BufferData? sent = null,
        bool dropConnection = false)
    {
        TcpHeaderReceived = tcpHeaderReceived;
        IpHeaderReceived = ipHeaderReceived;
        Received = received;

        TcpHeaderSent = tcpHeaderSent;
        IpHeaderSent = ipHeaderSent;
        Sent = sent;

        DropConnection = dropConnection;
    }

    public TcpHeader TcpHeaderReceived { get; }
    public IpHeader IpHeaderReceived { get; }
    public TcpHeader TcpHeaderSent { get; private set; }
    public IpHeader IpHeaderSent { get; private set; }
    public BufferData Received { get; }
    public BufferData? Sent { get; }
    public bool ContainsData() => Sent is not null;
    public bool ShouldReturnToClient() => TcpHeaderSent is not null && IpHeaderSent is not null;
    public bool DropConnection { get; }

    public static TcpProcessingContext Default = default;
    public TcpConnectionKey GetKey() => new TcpConnectionKey(IpHeaderReceived.SourceAddress, IpHeaderReceived.DestinationAddress,
        TcpHeaderReceived.SourcePort, TcpHeaderReceived.DestinationPort);

    public void SetTcpHeader(TcpHeader header) => TcpHeaderSent = header;
    public void SetIpHeader(IpHeader header) => IpHeaderSent = header;
}
