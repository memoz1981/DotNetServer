namespace DotNetServer.TCP.TCP;
public enum TcpOptionsKind
{
    EndOfOptionsList = 0, NoOp = 1, MaximumSegmentSize = 2,
    WindowScale = 3, SackPermitted = 4, SACK = 5, TimeStamp = 8,
    UserTimeoutOption = 28, TcpAuthentication = 29, MultipathTcp = 30
}
