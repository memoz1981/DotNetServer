namespace DotNetServer.TCP.Services;

//for now this service assumed one packet per request, later will be extended...
// check if it needs to be disposable
public class TcpConnection 
{
    public TcpConnection(
        ITcpConnectionManager connectionManager,
        TcpConnectionKey key,
        TcpConnectionState state,
        ushort? maximumSegmentSize,
        byte? windowScale,
        bool sackPermitted,
        List<(uint, uint)> sackBlocks,
        uint? timestampValue,
        uint? timestampEchoReply,
        ushort? timeoutInMs)
    {
        Key = key;
        State = state;
        MaximumSegmentSize = maximumSegmentSize;
        WindowScale = windowScale;
        SackPermitted = sackPermitted;
        SackBlocks = sackBlocks;
        TimestampValue = timestampValue;
        TimestampEchoReply = timestampEchoReply;
        TimeoutInMs = timeoutInMs;
        _connectionManager = connectionManager;
    }

    public TcpConnectionKey Key { get; }
    public TcpConnectionState State { get; }
    public ushort? MaximumSegmentSize { get; }
    public byte? WindowScale { get; }
    public bool? SackPermitted { get; }
    public List<(uint, uint)> SackBlocks { get; }
    public uint? TimestampValue { get; }
    public uint? TimestampEchoReply { get; }
    public ushort? TimeoutInMs { get; }
    private readonly ITcpConnectionManager _connectionManager;

    public async Task<(HttpData httpData, bool shouldReturn, bool shouldDropConnection)> HandleRequest(TcpData tcpData) =>
        throw new NotImplementedException();

    public async Task<TcpData> Send(HttpData httpData) => throw new NotImplementedException(); 
}
