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

    public async Task<(HttpData httpData, bool shouldReturn)> HandleRequest(TcpData tcpData) =>
        throw new NotImplementedException();

    public async Task<TcpData> Send(HttpData httpData) => throw new NotImplementedException(); 
}
