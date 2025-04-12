using DotNetServer.TCP.IP;
using DotNetServer.TCP.TCP;

namespace DotNetServer.TCP.Services;

//for now this service assumed one packet per request, later will be extended...
// check if it needs to be disposable
public class TcpConnection 
{
    public TcpConnectionKey Key { get; }
    public TcpConnectionState State { get; }
    public uint SequenceNumber { get;}
    public uint AcknowledgementNumber { get; }
    public int WindowSize { get; }

    public async Task<(byte[] httpData, bool shouldReturn)> HandleRequest(IpHeader ipHeader,
        TcpHeader tcpHeader, (byte[] data, int length) dataReceived, int dataStartIndex) => throw new NotImplementedException();  
}
