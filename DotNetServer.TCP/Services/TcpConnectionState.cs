namespace DotNetServer.TCP.Services;

public enum TcpConnectionState
{
    None,
    SynReceived,
    SynAckSent,
    Established,
    FinWait,
    CloseWait,
    LastAck,
    TimeWait,
    Closed,
}
