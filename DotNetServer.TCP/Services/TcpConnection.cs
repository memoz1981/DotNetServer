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
        _key = key;
        _state = state;
        _maximumSegmentSize = maximumSegmentSize;
        _windowScale = windowScale;
        _sackPermitted = sackPermitted;
        _sackBlocks = sackBlocks;
        _timestampValue = timestampValue;
        _timestampEchoReply = timestampEchoReply;
        _timeoutInMs = timeoutInMs;
        _connectionManager = connectionManager;
    }

    private readonly TcpConnectionKey _key;
    private TcpConnectionState _state;
    private ushort? _maximumSegmentSize;
    private byte? _windowScale;
    private bool? _sackPermitted;
    private List<(uint, uint)> _sackBlocks;
    private uint? _timestampValue;
    private uint? _timestampEchoReply;
    private ushort? _timeoutInMs;
    private readonly ITcpConnectionManager _connectionManager;

    public async Task<(HttpData httpData, bool shouldReturn, bool shouldDropConnection)> HandleRequest(
        TcpData tcpData)
    {

    }

    public async Task<TcpData> Send(HttpData httpData) => throw new NotImplementedException(); 
}
