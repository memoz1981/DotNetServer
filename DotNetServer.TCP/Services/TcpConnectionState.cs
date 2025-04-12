namespace DotNetServer.TCP.Services;

public enum TcpConnectionState
{
    None,
    Closed,
    SynSent,
    SynReceived,
    Established,
    FinWait,
    CloseWait,
    LastAck,
    TimeWait
}
