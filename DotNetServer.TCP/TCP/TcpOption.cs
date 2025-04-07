namespace DotNetServer.TCP.TCP;
public abstract class TcpOption
{
    protected TcpOption(TcpOptionsKind kind, int length)
    {
        Kind = kind;
        Length = length;
    }

    public TcpOptionsKind Kind { get; }
    public int Length { get; }
}

public sealed class TcpOptionNone : TcpOption
{
    public TcpOptionNone() : base(TcpOptionsKind.EndOfOptionsList, 1) {}
}

public sealed class TcpOptionNoOp : TcpOption
{
    public TcpOptionNoOp() : base(TcpOptionsKind.NoOp, 1) { }
}

public sealed class TcpOptionMss : TcpOption
{
    public TcpOptionMss(ushort maximumSegmentSize) : base(TcpOptionsKind.MaximumSegmentSize, 4)
    {
        MaximumSegmentSize = maximumSegmentSize;
    }

    public ushort MaximumSegmentSize { get; }
}

public sealed class TcpOptionWindowScale : TcpOption
{
    public TcpOptionWindowScale(byte windowScale) : base(TcpOptionsKind.MaximumSegmentSize, 3)
    {
        WindowScale = windowScale;
    }

    public byte WindowScale { get; }
}

public sealed class TcpOptionsSackPermitted : TcpOption
{
    public TcpOptionsSackPermitted() : base(TcpOptionsKind.SackPermitted, 2) { }
}

public sealed class TcpOptionsSack : TcpOption
{
    public TcpOptionsSack(int length, List<(uint, uint)> blocks) : base(TcpOptionsKind.SACK, length)
    {
        Blocks = blocks;
    }

    public List<(uint, uint)> Blocks { get; }
}

public sealed class TcpOptionsTimestamp : TcpOption
{
    public TcpOptionsTimestamp(uint timestampValue, uint timestampEchoReply) : base(TcpOptionsKind.TimeStamp, 10)
    {
        TimestampValue = timestampValue;
        TimestampEchoReply = timestampEchoReply;
    }

    public uint TimestampValue { get; }
    public uint TimestampEchoReply { get; }
}

public sealed class TcpOptionUserTimeout : TcpOption
{
    public TcpOptionUserTimeout(uint timeoutInMs) : base(TcpOptionsKind.UserTimeoutOption, 4)
    {
        TimeoutInMs = timeoutInMs;
    }

    public uint TimeoutInMs { get; }
}

public sealed class TcpOptionAuthenticated : TcpOption
{
    public TcpOptionAuthenticated() : base(TcpOptionsKind.TcpAuthentication, -1)
    {
        throw new ArgumentException("will be implemented later"); 
    }
}

public sealed class TcpOptionMultipath : TcpOption
{
    public TcpOptionMultipath() : base(TcpOptionsKind.MultipathTcp, -1)
    {
        throw new ArgumentException("will be implemented later");
    }
}
